using System;
using Lockstep.Client.Interfaces;
using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;    

namespace Lockstep.Client
{
    public class Simulation
    {
        public event EventHandler Started;
        public event Action<uint> Ticked;

        private readonly ISystems _systems;

        private readonly IDataReceiver _dataReceiver;
        private readonly FrameBuffer _frameBuffer = new FrameBuffer();

        public Simulation(ISystems systems, IDataReceiver dataReceiver)
        {
            _systems = systems;
            _systems.SetFrameBuffer(_frameBuffer);

            _dataReceiver = dataReceiver;

            _dataReceiver.InitReceived += OnInitReceived;
            _dataReceiver.FrameReceived += OnFrameReceived;
        }
        private void OnInitReceived(object sender, EventArgs e)
        {
            _systems.Initialize();
            Started?.Invoke(this, EventArgs.Empty);
        }

        private void OnFrameReceived(object sender, Frame e)
        {
            _frameBuffer.Insert(e);

            //TODO: only for debugging, frames should be executed later in fixed update, in case frames don't arrive in time this would halt the simulation
            Tick();
        }

        private void Tick()
        {                                       
            _systems.Tick();
            Ticked?.Invoke(_frameBuffer.ItemIndex);                                                                
        }  

        public void Execute(ICommand command)
        {
            _dataReceiver.Receive(command);
        }         
    }
}