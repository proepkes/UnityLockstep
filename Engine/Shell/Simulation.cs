using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Lockstep.Core;
using Lockstep.Core.Services;
using Lockstep.Game.Interfaces;
using Simulation.Behaviour.Services;

namespace Lockstep.Game.Simulation
{
    public class Simulation
    {
        public event EventHandler Started;

        public Contexts Contexts { get; }   

        /// <summary>
        /// Stores all inputs including the tick in which the input was added. Can be used to exactly re-simulate a game (including rollback/prediction)
        /// </summary>
        public GameLog GameLog { get; } = new GameLog();

        public byte LocalActorId { get; private set; }     

        public bool Running { get; private set; }

        public uint CurrentTick => _world.Tick;

        public ServiceContainer Services { get; }

        private float _tickDt;
        private float _accumulatedTime;

        private World _world;
        private readonly ICommandQueue _commandQueue;

        private readonly ILogService _logger;

                                                                                                       
        public Simulation(Contexts contexts, ICommandQueue commandQueue)
        {
            _commandQueue = commandQueue;

            Contexts = contexts;
            Services = new ServiceContainer();   

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes().Where(type => type.GetInterfaces().Any(intf => intf.FullName != null && intf.FullName.Equals(typeof(IService).FullName))))
                {
                    Services.Register((IService)Activator.CreateInstance(type));                             
                }
            }
                      
            _logger = Services.Get<ILogService>();
        }


        public void Start(int targetFps, byte localActorId, byte[] allActors)
        {                             
            LocalActorId = localActorId;     

            _tickDt = 1000f / targetFps;
            _world = new World(Contexts, Services, allActors);

            Running = true;

            Started?.Invoke(this, EventArgs.Empty);
        }


        public void Update(float elapsedMilliseconds)
        {
            if (!Running)
            {
                return;
            }

            SyncCommandBuffer();

            _accumulatedTime += elapsedMilliseconds;

            while (_accumulatedTime >= _tickDt)
            {
                _world.Predict();

                _accumulatedTime -= _tickDt;
            }
        }

        public void Execute(ICommand command)
        {
            _commandQueue.Enqueue(CurrentTick, LocalActorId, command);
        }

        private void CreateInput(uint tickId, byte actor, IEnumerable<ICommand> input)
        {
            var inputList = input.ToList();

            GameLog.Add(CurrentTick, new Input(tickId, actor, inputList));

            foreach (var command in inputList)
            {
                var inputEntity = Contexts.input.CreateEntity();
                command.Execute(inputEntity);

                inputEntity.AddTick(tickId);
                inputEntity.AddActorId(actor);
            }

            //TODO: after adding input, order the commands by timestamp => if commands intersect, the first one should win, timestamp should be added by server, RTT has to be considered
            //ordering by timestamp requires loopback functionality because we have to wait for server-response; at the moment commands get distributed to all clients except oneself
            //if a command comes back from server and it was our own command, the local command has to be overwritten instead of just adding it (like it is at the moment)
        }

        private void SyncCommandBuffer()
        {
            var commands = _commandQueue.Dequeue();

            if (commands.Count > 0)
            {
                var lastInputFrame = commands.Keys.Max();

                //We guess everything was predicted correctly (except the last received frame)
                var firstMispredictedFrame = lastInputFrame;

                //Store new input
                foreach (var (tick, commandsPerActor) in commands)
                {
                    foreach (var (actorId, actorCommands) in commandsPerActor)
                    {
                        _logger.Trace(() => actorId + " >> " + tick + ": " + actorCommands.Count());

                        CreateInput(tick, actorId, actorCommands);
                        if (tick < firstMispredictedFrame && actorId != LocalActorId)
                        {
                            firstMispredictedFrame = tick;
                        }
                    }
                }

                _logger.Trace(() => ">>>Input from " + firstMispredictedFrame + " to " + lastInputFrame);

                //Only rollback if the mispredicted frame was in the past (the frame can be in the future due to high lag compensation)
                if (firstMispredictedFrame < CurrentTick)
                {
                    var targetTick = CurrentTick;

                    _world.RevertToTick(firstMispredictedFrame);

                    //Restore last local state       
                    while (CurrentTick <= lastInputFrame && CurrentTick < targetTick)
                    {
                        _world.Simulate();
                    }

                    while (CurrentTick < targetTick)
                    {
                        _world.Predict();
                    }
                }
            }
        }              
    }
}
