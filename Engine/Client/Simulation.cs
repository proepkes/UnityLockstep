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

        private readonly ISystems _systems;

        private readonly IDataReceiver _dataReceiver;
        private readonly FrameBuffer _frameBuffer = new FrameBuffer();

        private float _tickDt;
        private float _accumulatedTime;

        public Simulation(ISystems systems, IDataReceiver dataReceiver)
        {
            _systems = systems;
            _systems.SetFrameBuffer(_frameBuffer);

            _dataReceiver = dataReceiver;

            _dataReceiver.InitReceived += OnInitReceived;
            _dataReceiver.FrameReceived += OnFrameReceived;
        }

        public void Execute(ICommand command)
        {
            _dataReceiver.Receive(command);
        }

        public void Update(float deltaTime, ILogService logger)
        {
            if (!Running)                        
            {
                return;
            }        

            _accumulatedTime += deltaTime;

            //TODO: adjust _tickDt depending on buffersize
            while (_accumulatedTime >= _tickDt && _frameBuffer.Count - _frameBuffer.ItemIndex > 2)
            {
                logger.Warn("tick " + (_frameBuffer.Count - _frameBuffer.ItemIndex));
                Tick();
                                             
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
            _frameBuffer.Insert(e); 
        }

        private void Tick()
        {                                       
            _systems.Tick();
            Ticked?.Invoke(_frameBuffer.ItemIndex);                                                                
        }        
    }
}