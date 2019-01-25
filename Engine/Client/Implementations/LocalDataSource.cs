using System;
using Lockstep.Client.Interfaces;
using Lockstep.Core.Data;

namespace Lockstep.Client.Implementations
{
    public class LocalDataSource : IDataSource
    {
        public event EventHandler InitReceived;
        public event EventHandler<Frame> FrameReceived;

        public void Init()
        {
            InitReceived?.Invoke(this, EventArgs.Empty);
        }

        public void AddFrame(Frame frame)
        {             
            FrameReceived?.Invoke(this, frame);
        }

        public void Receive(ICommand command)
        {                                       
            FrameReceived?.Invoke(this, new Frame{Commands = new []{command}});
        }
    }
}
