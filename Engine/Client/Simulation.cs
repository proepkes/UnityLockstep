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
        public int LagCompensation { get; set; } = 10;

        public byte LocalPlayerId { get; private set; }   

        public bool Running { get; set; }   

        public long CurrentTick { get; private set; }

        public readonly ICommandBuffer CommandBuffer;   

        public float _tickDt;
        public float _accumulatedTime;
        private readonly ISystems _systems;
        private readonly List<ICommand> _temporaryCommandBuffer = new List<ICommand>(20);


        public Simulation(ISystems systems, ICommandBuffer commandBuffer, ILogService logger)
        {
            _systems = systems;
            _systems.CommandBuffer = commandBuffer;

            CommandBuffer = commandBuffer;          
            CommandBuffer.Inserted += (playerId, frameNumber, command) =>
            {          
                if (frameNumber < CurrentTick)
                {
                    logger.Warn($"Rollback required, because we are at frame {CurrentTick} but player {playerId} sent us input for frame {frameNumber}");
                } 
            };
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

            CommandBuffer.Insert(LocalPlayerId, CurrentTick + LagCompensation, frameCommands);

            _systems.Tick();

            Ticked?.Invoke(CurrentTick);

            CurrentTick++;
        }
    }
}