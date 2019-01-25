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
        public event Action<uint, Frame> Ticked;

        private readonly ISystems _systems;

        private readonly IDataSource _dataSource;
        private readonly FrameBuffer _frameBuffer = new FrameBuffer();

        public Simulation(ISystems systems, IDataSource dataSource)
        {
            _systems = systems;
            _dataSource = dataSource;

            _dataSource.InitReceived += OnInitReceived;
            _dataSource.FrameReceived += OnFrameReceived;
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
            var frame = _frameBuffer.GetNext();

            _systems.SetFrame(frame);
            _systems.Tick();
            Ticked?.Invoke(_frameBuffer.ItemIndex, frame);                                                                
        }  

        public void Execute(ICommand command)
        {
            _dataSource.Receive(command);
        }         
    }
}