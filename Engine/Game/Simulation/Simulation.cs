using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lockstep.Core;
using Lockstep.Core.Commands;
using Lockstep.Core.Services;
using Lockstep.Core.World;
using Lockstep.Game.Services;
using Lockstep.Game.Services.Navigation;
using Timer = Lockstep.Game.Utility.Timer;

namespace Lockstep.Game.Simulation
{
    public class Simulation
    {
        public event EventHandler Started;

        /// <summary>
        /// Stores all inputs including the tick in which the input was added. Can be used to exactly re-simulate a game (including rollback/prediction)
        /// </summary>
        public GameLog GameLog { get; } = new GameLog();

        public byte LocalActorId { get; private set; }
        public byte[] AllActorIds { get; private set; }

        public bool Running { get; private set; }

        public uint CurrentTick => _world.CurrentTick;

        public ServiceContainer Services => _world.Services;

        private float _tickDt;
        private float _accumulatedTime;

        private readonly IWorld _world;      
        private readonly ICommandBuffer _commandBuffer;

        private readonly ILogService _logger;

        public Simulation(Contexts contexts, ICommandBuffer commandBuffer, params IService[] additionalServices)
        {
            _commandBuffer = commandBuffer;   
            _world = WorldFactory.CreateWorld(contexts, additionalServices.Concat(new IService[]
            {
                new DefaultHashService(),
                new DefaultViewService(),
                new DefaultNavigationService(),
                new DefaultSnapshotIndexService(),
                new DefaultDebugService(),
                new DefaultLogService()
            }).ToArray());

            _logger = _world.Services.Get<ILogService>();
        }
               

        public void Start(int targetFps, byte localActorId, byte[] allActors)
        {
            AllActorIds = allActors;
            LocalActorId = localActorId;

            _tickDt = 1000f / targetFps;

            _world.Initialize(allActors);

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
            _commandBuffer.Insert(CurrentTick, LocalActorId, command);
        }

        private void CreateInput(uint tickId, byte actor, List<ICommand> input)
        {
            GameLog.Add(CurrentTick, tickId, actor, input.ToArray());

            foreach (var command in input)
            {
                var inputEntity = _world.CreateInputEntity();
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
            var commands = _commandBuffer.GetChanges();

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
                        _logger.Trace(() => actorId + " >> " + tick + ": " + actorCommands.Count);

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

        /// <summary>
        /// Experimental
        /// </summary>
        private void StartAsThread()
        {
            new Thread(Loop) { IsBackground = true }.Start();
        }

        /// <summary>
        /// Experimental
        /// </summary>  
        private void Loop()
        {
            var timer = new Timer();

            Running = true;

            timer.Start();

            while (Running)
            {
                Update(timer.Tick());

                Thread.Sleep(1);
            }
        }
    }
}
