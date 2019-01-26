using System;
using Lockstep.Client.Interfaces;
using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;
using Lockstep.Network.Messages;

namespace Lockstep.Client
{
    public class Simulation
    {                                       
        public event Action<uint> Ticked;

        public bool Running { get; set; }

        public IFrameBuffer FrameBuffer { get; } = new FrameBuffer();

        private readonly ISystems _systems;

        private readonly IDataReceiver _dataReceiver;                   

        public float _tickDt;
        public float _accumulatedTime;    

        public Simulation(ISystems systems, IDataReceiver dataReceiver)
        {
            _systems = systems;
            _systems.SetFrameBuffer(FrameBuffer);

            _dataReceiver = dataReceiver;

            _dataReceiver.InitReceived += OnInitReceived;
            _dataReceiver.FrameReceived += OnFrameReceived;
        }

        public void Execute(ICommand command)
        {
            _dataReceiver.Receive(command);
        }

        public void Update(float deltaTime)
        {                             
            if (!Running)                        
            {
                return;
            }        

            _accumulatedTime += deltaTime;

            //TODO: adjust _tickDt depending on buffersize
            while (_accumulatedTime >= _tickDt)
            {            
                 //Tick();          

                _accumulatedTime -= _tickDt;
            }                 
        }

        private void OnInitReceived(object sender, Init init)
        {
            _tickDt = 1000f / init.TargetFPS;

            _systems.Initialize();

            Running = true;                         
        }

        private void OnFrameReceived(object sender, Frame e)
        {
            FrameBuffer.Insert(e);

            Tick();
        }

        private void Tick()
        {                                       
            _systems.Tick();
            Ticked?.Invoke(FrameBuffer.ItemIndex);                                                                
        }        
    }
}