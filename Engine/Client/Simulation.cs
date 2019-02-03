using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lockstep.Client.Interfaces;
using Lockstep.Core.Interfaces;
using Lockstep.Network.Messages;
using Lockstep.Network.Utils;

namespace Lockstep.Client
{                    
    public class Simulation
    {                                        
        /// <summary>
        /// Amount of ticks until a command gets executed locally
        /// </summary>
        public uint LagCompensation { get; set; }

        public bool SendCommandsToBuffer { get; set; } = true;

        public bool Running { get; private set; }

        public byte LocalPlayerId { get; private set; } 

        private float _tickDt;
        private float _accumulatedTime;    

        private readonly IWorld _world;
        private readonly ICommandBuffer _remoteCommandBuffer;                         

        private readonly List<ICommand> _commandCache = new List<ICommand>();

        public Simulation(IWorld world, ICommandBuffer remoteCommandBuffer)
        {
            _world = world;     
            _remoteCommandBuffer = remoteCommandBuffer;
        }

        public void Initialize(Init init)
        {             
            _tickDt = 1000f / init.TargetFPS;
            LocalPlayerId = init.ActorID;
                                                                     
            _world.Initialize(init.AllActors);

            Running = true;
        }    

        public void Execute(ICommand command)
        {
            if (!Running)
            {
                return;
            }

            if (command is ISerializableCommand s)
            {
                var er = new Serializer();
                s.Serialize(er);
                s.Deserialize(new Deserializer(er.Data));
            }

            lock (_commandCache)
            {
                _commandCache.Add(command);
            }            
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
                Tick();

                _accumulatedTime -= _tickDt;
            }                 
        }

        private void Tick()
        {                              
            lock (_commandCache)
            {
                if (_commandCache.Count > 0)
                {
                    _world.AddInput(_world.CurrentTick + LagCompensation, LocalPlayerId, _commandCache);
                    if (SendCommandsToBuffer)
                    {
                        _remoteCommandBuffer.Insert(_world.CurrentTick + LagCompensation, LocalPlayerId, _commandCache.ToArray()); 
                    }

                    _commandCache.Clear();  
                }
            }                                                                                                          

            _world.Predict();     
        }

        private void SyncCommandBuffer()
        {
            var commands = _remoteCommandBuffer.GetChanges();
  
            if (commands.Count > 0)
            {                                                                                                                       
                //We guess everything was predicted correctly (except the last received frame)
                var firstMispredictedFrame = commands.Keys.Min();
                var lastInputFrame = commands.Keys.Max();


                _world.Services.Get<ILogService>().Trace(">>>Input from " + firstMispredictedFrame + " to " + lastInputFrame);

                foreach (var tick in commands.Keys)
                {
                    foreach (var actorId in commands[tick].Keys)
                    {
                        _world.AddInput(tick, actorId, commands[tick][actorId]);
                    }
                }           

                //Only rollback if the mispredicted frame was in the past (the frame can be in the future due to high lag compensation)
                if (firstMispredictedFrame <= _world.CurrentTick)
                {      
                    var targetTick = _world.CurrentTick;  
                                                                                                                                                                     
                    _world.RevertToTick(firstMispredictedFrame);

                    //Restore last local state       
                    while (_world.CurrentTick <= lastInputFrame)
                    {
                        _world.Simulate();
                    }

                    _world.Services.Get<ILogService>().Trace(">>>Predicting up to " + targetTick);
                    while (_world.CurrentTick < targetTick)
                    {   
                        _world.Predict();      
                    }

                    _world.Services.Get<ILogService>().Trace(">>>Done: now at " + _world.CurrentTick);
                }                                          
            }   
        }


        /// <summary>
        /// Experimental
        /// </summary>
        public void StartAsThread()
        {
            new Thread(Loop) { IsBackground = true }.Start();
        }

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