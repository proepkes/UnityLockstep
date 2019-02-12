using System;
using System.Collections.Generic;
using System.Linq;
using Lockstep.Common.Logging;
using Lockstep.Core.Logic;
using Lockstep.Core.Logic.Interfaces;
using Lockstep.Game.Features.Cleanup;
using Lockstep.Game.Features.Input;
using Lockstep.Game.Features.Navigation;

namespace Lockstep.Game
{
    public class Simulation
    {
        public event EventHandler Started;

        public Contexts Contexts { get; }   

        public GameLog GameLog { get; } = new GameLog();

        public byte LocalActorId { get; private set; }     

        public bool Running { get; private set; }
                                           
        public ServiceContainer Services { get; }

        private float _tickDt;
        private float _accumulatedTime;

        private World _world;
        private readonly ICommandQueue _commandQueue;  
        
        private readonly List<ICommand> _localCommandBuffer = new List<ICommand>();
                                                                                                       
        public Simulation(Contexts contexts, ICommandQueue commandQueue, params IService[] services)
        {
            _commandQueue = commandQueue;

            Contexts = contexts;
            Services = new ServiceContainer();                    

            foreach (var service in services)
            {                         
                Services.Register(service);
            }                                          
        }


        public void Start(int targetFps, byte localActorId, byte[] allActors)
        {
            GameLog.LocalActorId = localActorId;
            GameLog.AllActorIds = allActors;

            LocalActorId = localActorId;     

            _tickDt = 1000f / targetFps;
            _world = new World(Contexts, allActors, CreateFeatures());

            Running = true;

            Started?.Invoke(this, EventArgs.Empty);
        }


        public void Update(float elapsedMilliseconds)
        {
            if (!Running)
            {
                return;
            }
            
            _accumulatedTime += elapsedMilliseconds;

            while (_accumulatedTime >= _tickDt)
            {
                lock (_localCommandBuffer)
                {
                    if (_localCommandBuffer.Any())
                    {          
                        _commandQueue.Enqueue(new Input(_world.Tick, LocalActorId, _localCommandBuffer.ToArray()));
                        _localCommandBuffer.Clear();
                    }

                    ProcessInputQueue();

                    _world.Predict();
                }

                _accumulatedTime -= _tickDt;
            }
        }

        public void Execute(ICommand command)
        {
            if (!Running)
            {
                return;
            }

            lock (_localCommandBuffer)
            {
                _localCommandBuffer.Add(command);
            }
        }

        private Feature[] CreateFeatures()
        {
            var input = new Feature("Input");
            //TODO: Add InputValidationSystem  
            input.Add(new ExecuteSpawnInput(Contexts, Services));
            input.Add(new VerifySelectionIdExists(Contexts));
            input.Add(new ExecuteNavigationInput(Contexts, Services));
            //TODO: Add CleanupInput that removes input of validated frames (no rollback required => can be removed)


            var navigation = new Feature("Navigation");                      
            navigation.Add(new NavigationTick(Contexts, Services));         

            var cleanup = new Feature("Cleanup");
            cleanup.Add(new RemoveDestroyedEntitiesFromView(Contexts, Services));

            return new[] {input, navigation, cleanup};
        }

        
        private void ProcessInputQueue()
        {
            var inputs = _commandQueue.Dequeue();

            if (inputs.Any())
            {
                //Store new input
                foreach (var input in inputs)
                {
                    GameLog.Add(_world.Tick, input);

                    foreach (var command in input.Commands)
                    {
                        Log.Trace(this, input.ActorId + " >> " + input.Tick + ": " + input.Commands.Count());

                        var inputEntity = Contexts.input.CreateEntity();
                        command.Execute(inputEntity);

                        inputEntity.AddTick(input.Tick);
                        inputEntity.AddActorId(input.ActorId);

                        //TODO: after adding input, order the commands by timestamp => if commands intersect, the first one should win, timestamp should be added by server, RTT has to be considered
                        //ordering by timestamp requires loopback functionality because we have to wait for server-response; at the moment commands get distributed to all clients except oneself
                        //if a command comes back from server and it was our own command, the local command has to be overwritten instead of just adding it (like it is at the moment)
                    }
                }

                var otherActorsInput = inputs.Where(input => input.ActorId != LocalActorId).ToList();
                if (otherActorsInput.Any())
                {
                    var firstRemoteInputTick = otherActorsInput.Min(input => input.Tick);
                    var lastRemoteInputTick = otherActorsInput.Max(input => input.Tick);

                    Log.Trace(this, ">>>Input from " + firstRemoteInputTick + " to " + lastRemoteInputTick);

                    //Only rollback if the mispredicted frame was in the past (the frame can be in the future e.g. due to high lag compensation)
                    if (firstRemoteInputTick < _world.Tick)
                    {
                        var targetTick = _world.Tick;

                        _world.RevertToTick(firstRemoteInputTick);

                        //Restore last local state       
                        while (_world.Tick <= lastRemoteInputTick && _world.Tick < targetTick)
                        {
                            _world.Simulate();
                        }

                        while (_world.Tick < targetTick)
                        {
                            _world.Predict();
                        }
                    }
                }
            }
        }              
    }
}
