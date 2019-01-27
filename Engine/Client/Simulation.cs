using System;                 
using Lockstep.Client.Implementations;
using Lockstep.Client.Interfaces;
using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;
using Lockstep.Network.Messages;

namespace Lockstep.Client
{
    public class Simulation
    {                                       
        public event Action<long> Ticked;

        public bool Running { get; set; }
                                                                      
        private readonly ISystems _systems;   

        public float _tickDt;
        public float _accumulatedTime;

        public long CurrentTick { get; private set; }

        public ICommandBuffer LocalCommandBuffer => _systems.CommandBuffer;
        public readonly ICommandBuffer RemoteCommandBuffer;

        private readonly object _currentTickLock = new object();


        public Simulation(ISystems systems, ICommandBuffer remoteCommandBuffer)
        {
            _systems = systems;
            _systems.CommandBuffer = new CommandBuffer();

            RemoteCommandBuffer = remoteCommandBuffer;
            RemoteCommandBuffer.Inserted += (l, command) =>
            {
                LocalCommandBuffer.Insert(l, command);
            };
        }

        public void Start(Init init)
        {             
            _tickDt = 1000f / init.TargetFPS;

            _systems.Initialize();

            Running = true;
        }   

        public void Execute(ICommand command)
        {                                             
            lock (_currentTickLock)
            {
                var executionTick = CurrentTick + 20;

                //LocalCommandBuffer.Insert(nextTick, command);
                RemoteCommandBuffer.Insert(executionTick, command);
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