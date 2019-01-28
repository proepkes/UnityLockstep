using System;
using System.Collections.Generic;
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
                                                           
        public readonly ICommandBuffer CommandBuffer;

        public float _tickDt;
        public float _accumulatedTime;
        private readonly ISystems _systems;
        private readonly List<ICommand> _temporaryCommandBuffer = new List<ICommand>(20);


        public Simulation(ISystems systems, ICommandBuffer commandBuffer)
        {
            _systems = systems;                       

            CommandBuffer = commandBuffer;    
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
                CommandBuffer.Insert(LocalPlayerId, _systems.CurrentTick + LagCompensation, frameCommands);
            }
                                     
            _systems.Tick(CommandBuffer.GetNext());

            Ticked?.Invoke(_systems.CurrentTick);      
        }

        private void SyncCommandBuffer()
        {
            //While buffer contains inputs from the past, rollback and execute them     

            if (CommandBuffer.NextFrameIndex < _systems.CurrentTick)
            {
                var targetTick = _systems.CurrentTick;

                _systems.RevertToTick(CommandBuffer.NextFrameIndex);

                while (CommandBuffer.NextFrameIndex <= targetTick)
                {
                    _systems.Tick(CommandBuffer.GetNext());
                }
            }  
            
        }
    }
}