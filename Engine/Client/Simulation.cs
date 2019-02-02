using System.Collections.Generic;
using System.Threading;
using Lockstep.Client.Interfaces;     
using Lockstep.Core.Interfaces;
using Lockstep.Network.Messages;

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
        private uint _lastValidatedFrame;

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
                                                                     
            _world.Initialize(LocalPlayerId);

            Running = true;
        }    

        public void Execute(ICommand command)
        {
            if (!Running)
            {
                return;
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
            var currentRemoteFrame = _remoteCommandBuffer.LastInsertedFrame;
  
            if (_lastValidatedFrame < currentRemoteFrame)
            {                                                                                                                       
                //We guess everything was predicted correctly (except the last received frame)
                var firstMispredictedFrame = currentRemoteFrame; 
                                                                        
                for (var remoteFrame = _lastValidatedFrame + 1; remoteFrame <= currentRemoteFrame; remoteFrame++)
                {
                    //All frames that have no commands were predicted correctly => increase remote frame
                    var allPlayerCommands = _remoteCommandBuffer.Get(remoteFrame);
                    if (allPlayerCommands.Count == 0)
                    {
                        continue;
                    }

                    if (firstMispredictedFrame > remoteFrame)
                    {
                        //Set the first mispredicted frame to the first frame which contains commands
                        firstMispredictedFrame = remoteFrame;
                    }

                    //TODO: if command contains entity-ids (which can be predicted) and due to rollback->fast-forward we generated local ids, the command's entity-ids have to be adjusted
                    //https://github.com/proepkes/UnityLockstep/wiki/Rollback-devlog
                    foreach (var playerCommands in allPlayerCommands)
                    {
                        _world.AddInput(remoteFrame, playerCommands.Key, playerCommands.Value);
                    }
                }

                //Only rollback if the mispredicted frame was in the past (the frame can be in the future due to high lag compensation)
                if (firstMispredictedFrame <= _world.CurrentTick)
                {      
                    var targetTick = _world.CurrentTick;  
                                                                                                                                                                     
                    _world.RevertToTick(firstMispredictedFrame);
                    _world.Simulate();  

                    //Restore last local state
                    while (_world.CurrentTick < targetTick)
                    {   
                        _world.Predict();      
                    }
                }

                _lastValidatedFrame = currentRemoteFrame;
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