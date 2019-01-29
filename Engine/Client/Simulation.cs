using System;
using System.Collections.Generic;
using System.Linq;
using Lockstep.Client.Implementations;
using Lockstep.Core.Data;
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
        public uint LagCompensation { get; set; } = 10;

        public byte LocalPlayerId { get; private set; }   

        public bool Running { get; set; }
                                                           
        public readonly ICommandBuffer RemoteCommandBuffer;
        private readonly ILogService _logger;

        public float _tickDt;
        public float _accumulatedTime;
        private readonly ISystems _systems;
        private readonly List<ICommand> _temporaryCommandBuffer = new List<ICommand>(20);


        public readonly ICommandBuffer LocalCommandBuffer = new CommandBuffer();
        public uint LastValidatedFrame = 0;


        public Simulation(ISystems systems, ICommandBuffer remoteCommandBuffer)
        {
            _systems = systems;                       

            RemoteCommandBuffer = remoteCommandBuffer;

            _logger = systems.Services.Get<ILogService>();
        }

        public void Start(Init init)
        {             
            _tickDt = 1000f / init.TargetFPS;
            LocalPlayerId = init.PlayerID;

            _systems.Initialize();

            Running = true;
        }   

        public void Execute(ICommand command)
        {
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
                LocalCommandBuffer.Insert(_systems.CurrentTick + LagCompensation, LocalPlayerId, frameCommands);
                RemoteCommandBuffer.Insert(_systems.CurrentTick + LagCompensation, LocalPlayerId,  frameCommands);     
            }
                                     
            _systems.Tick(LocalCommandBuffer.GetMany(_systems.CurrentTick));

            Ticked?.Invoke(_systems.CurrentTick);      
        }

        private void SyncCommandBuffer()
        {
            var currentRemoteFrame = RemoteCommandBuffer.LastInsertedFrame;
  
            if (LastValidatedFrame < currentRemoteFrame)
            {                  
                var revertTick = currentRemoteFrame;
                                                                        
                for (var remoteFrame = LastValidatedFrame + 1; remoteFrame <= currentRemoteFrame; remoteFrame++)
                {
                    var allPlayerCommands = RemoteCommandBuffer.Get(remoteFrame);
                    if (allPlayerCommands.Count <= 0)
                    {
                        continue;
                    }

                    if (remoteFrame < revertTick)
                    {
                        //Save the first tick which contains remote commands
                        revertTick = remoteFrame;
                    }

                    //Merge commands into the local command buffer
                    foreach (var commandPerPlayer in allPlayerCommands)
                    {
                        LocalCommandBuffer.Insert(remoteFrame, commandPerPlayer.Key, commandPerPlayer.Value.ToArray());
                    }
                }

                //Only rollback if we are ahead (network can be ahead when lag compensation is higher than lag itself)
                if (_systems.CurrentTick > revertTick)
                {      
                    var targetTick = _systems.CurrentTick;  
                    
                    _systems.RevertToTick(revertTick); 
                                                                         
                    while (_systems.CurrentTick < targetTick)
                    {            
                        _systems.Tick(LocalCommandBuffer.GetMany(_systems.CurrentTick));
                    }
                }

                LastValidatedFrame = currentRemoteFrame;
            }   
        }
    }
}