using System;
using Lockstep.Core.Data;

namespace Lockstep.Client.Interfaces
{
    public interface IDataReceiver
    {
        event EventHandler InitReceived;
        event EventHandler<Frame> FrameReceived;

        void Receive(ICommand command);
    }
}