using System;
using System.Collections.Generic;      
using Lockstep.Client.Implementations;
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

        public bool Running { get; private set; }

        public byte LocalPlayerId { get; private set; }


        private float _tickDt;
        private float _accumulatedTime;
        private uint _lastValidatedFrame;

        private readonly ITickable _world;
        private readonly ICommandBuffer _remoteCommandBuffer;
        private readonly ICommandBuffer _localCommandBuffer = new CommandBuffer();

        private readonly List<ICommand> _commandCache = new List<ICommand>(20); 

        public Simulation(ITickable world, ICommandBuffer remoteCommandBuffer)
        {
            _world = world;     
            _remoteCommandBuffer = remoteCommandBuffer;      
        }

        public void Start(Init init)
        {             
            _tickDt = 1000f / init.TargetFPS;
            LocalPlayerId = init.PlayerID;

            _world.Initialize();

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

        public void Update(float deltaTime)
        {                             
            if (!Running)                        
            {
                return;
            }

            SyncCommandBuffer();

            _accumulatedTime += deltaTime; 

            while (_accumulatedTime >= _tickDt)
            {                                                                                
                Tick();

                _accumulatedTime -= _tickDt;
            }                 
        }

        private void Tick()
        {            
            ICommand[] frameCommands;
            lock (_commandCache)
            {
                frameCommands = _commandCache.ToArray();
                _commandCache.Clear();
            }

            if (frameCommands.Length > 0)
            {
                _localCommandBuffer.Insert(_world.CurrentTick + LagCompensation, LocalPlayerId, frameCommands);
                _remoteCommandBuffer.Insert(_world.CurrentTick + LagCompensation, LocalPlayerId,  frameCommands);     
            }

            _world.Tick(_localCommandBuffer.GetMany(_world.CurrentTick));     
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
                    if (allPlayerCommands.Count <= 0)
                    {
                        continue;
                    }

                    if (firstMispredictedFrame > remoteFrame)
                    {
                        //Set the first mispredicted frame to the first frame which contains commands
                        firstMispredictedFrame = remoteFrame;
                    }

                    //Merge commands into the local command buffer
                    foreach (var commandPerPlayer in allPlayerCommands)
                    {
                        //TODO: order by timestamp in case of multiple commands in the same frame => if commands intersect, the first one should win, timestamp should be added by server, RTT has to be considered
                        //ordering is enough, validation should take place in the simulation(core)                                                                    
                        _localCommandBuffer.Insert(remoteFrame, commandPerPlayer.Key, commandPerPlayer.Value.ToArray());
                    }
                }

                //Only rollback if the mispredicted frame was in the past (the frame can be in the future due to high lag compensation)
                if (firstMispredictedFrame < _world.CurrentTick)
                {      
                    var targetTick = _world.CurrentTick;  
                                                                                                                                                                     
                    _world.RevertToTick(firstMispredictedFrame);

                    var validFrame = firstMispredictedFrame;

                    //Execute all commands again, beginning from the first frame that contains remote input up to our last local state
                    while (validFrame <= targetTick)
                    {   
                        _world.Tick(_localCommandBuffer.GetMany(validFrame));
                        validFrame++;
                    }
                }

                _lastValidatedFrame = currentRemoteFrame;
            }   
        }
    }
}