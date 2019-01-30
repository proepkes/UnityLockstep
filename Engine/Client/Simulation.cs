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
        public event Action<long> Ticked;     

        /// <summary>
        /// Amount of ticks until a command gets executed
        /// </summary>
        public uint LagCompensation { get; set; }

        public byte LocalPlayerId { get; private set; }   

        public bool Running { get; set; }
                                                           
        public readonly ICommandBuffer RemoteCommandBuffer;     

        public float _tickDt;
        public float _accumulatedTime;
        private readonly ITickable _tickable;
        private readonly List<ICommand> _temporaryCommandBuffer = new List<ICommand>(20);


        public readonly ICommandBuffer LocalCommandBuffer = new CommandBuffer();
        public uint LastValidatedFrame = 0;


        public Simulation(ITickable tickable, ICommandBuffer remoteCommandBuffer)
        {
            _tickable = tickable;                       

            RemoteCommandBuffer = remoteCommandBuffer;      
        }

        public void Start(Init init)
        {             
            _tickDt = 1000f / init.TargetFPS;
            LocalPlayerId = init.PlayerID;

            _tickable.Initialize();

            Running = true;
        }   

        public void Execute(ICommand command)
        {
            if (!Running)
            {
                return;
            }

            lock (_temporaryCommandBuffer)
            {
                _temporaryCommandBuffer.Add(command);
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
            lock (_temporaryCommandBuffer)
            {
                frameCommands = _temporaryCommandBuffer.ToArray();
                _temporaryCommandBuffer.Clear();
            }

            if (frameCommands.Length > 0)
            {
                LocalCommandBuffer.Insert(_tickable.CurrentTick + LagCompensation, LocalPlayerId, frameCommands);
                RemoteCommandBuffer.Insert(_tickable.CurrentTick + LagCompensation, LocalPlayerId,  frameCommands);     
            }

            _tickable.Tick(LocalCommandBuffer.GetMany(_tickable.CurrentTick));

            Ticked?.Invoke(_tickable.CurrentTick);      
        }

        private void SyncCommandBuffer()
        {
            var currentRemoteFrame = RemoteCommandBuffer.LastInsertedFrame;
  
            if (LastValidatedFrame < currentRemoteFrame)
            {
                //We guess everything was predicted correctly
                var firstMispredictedFrame = currentRemoteFrame; 
                                                                        
                for (var remoteFrame = LastValidatedFrame + 1; remoteFrame <= currentRemoteFrame; remoteFrame++)
                {
                    //All frames that have no commands were predicted correctly => increase remote frame
                    var allPlayerCommands = RemoteCommandBuffer.Get(remoteFrame);
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
                        LocalCommandBuffer.Insert(remoteFrame, commandPerPlayer.Key, commandPerPlayer.Value.ToArray());
                    }
                }

                //Only rollback if the mispredicted frame was in the past (the frame can be in the future due to high lag compensation)
                if (firstMispredictedFrame < _tickable.CurrentTick)
                {      
                    var targetTick = _tickable.CurrentTick;  
                                                                                                                                                                     
                    _tickable.RevertToTick(firstMispredictedFrame);

                    var validFrame = firstMispredictedFrame;

                    //Execute all commands again, beginning from the first frame that contains remote input up to our last local state
                    while (validFrame <= targetTick)
                    {   
                        _tickable.Tick(LocalCommandBuffer.GetMany(validFrame));
                        validFrame++;
                    }
                }

                LastValidatedFrame = currentRemoteFrame;
            }   
        }
    }
}