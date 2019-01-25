using System;
using Lockstep.Client.Interfaces;
using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;    

namespace Lockstep.Client
{
    /// <summary>
    /// This simulation listens for received data from the client and reacts accordingly. 'Executed' commands are first sent to the server.
    /// The final execution is done when the corresponding frame-packet arrives - this adds minimum 1 RTT delay to commands.
    /// </summary>
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