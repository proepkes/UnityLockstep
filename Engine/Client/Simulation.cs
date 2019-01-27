using System;                     
using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;
using Lockstep.Network.Messages;

namespace Lockstep.Client
{
    public class Simulation
    {                                       
        public event Action<long> Ticked;

        public byte LocalPlayerId { get; private set; }

        public bool Running { get; set; }
                                                                      
        private readonly ISystems _systems;   

        public float _tickDt;
        public float _accumulatedTime;

        public long CurrentTick { get; private set; }

        /// <summary>
        /// Amount of ticks until a command gets executed
        /// </summary>
        public int LagCompensation { get; set; } = 10;
                                                                                 
        public readonly ICommandBuffer CommandBuffer;
        private readonly ILogService _logger;

        private readonly object _currentTickLock = new object();


        public Simulation(ISystems systems, ICommandBuffer commandBuffer, ILogService logger)
        {
            _systems = systems;
            _systems.CommandBuffer = commandBuffer;

            CommandBuffer = commandBuffer;
            _logger = logger;
            CommandBuffer.Inserted += (playerId, frameNumber, command) =>
            {
                lock (_currentTickLock)
                {
                    if (playerId != LocalPlayerId && frameNumber < CurrentTick)
                    {
                        logger.Warn($"Rollback required, because we are at frame {CurrentTick} but player {playerId} sent us input for frame {frameNumber}");
                    }
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
            lock (_currentTickLock)
            {
                var executionTick = CurrentTick + LagCompensation;    

                CommandBuffer.Insert(LocalPlayerId, executionTick, command);
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
                lock (_currentTickLock)
                {
                    Tick(); 
                }        

                _accumulatedTime -= _tickDt;
            }                 
        }
     

        private void Tick()
        {                             
            _systems.Tick();
            Ticked?.Invoke(CurrentTick);

            CurrentTick++;
        }

    }
}