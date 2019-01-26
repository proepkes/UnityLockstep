using System;
using Lockstep.Core.Data;
using Lockstep.Network.Messages;

namespace Lockstep.Client.Interfaces
{
    public interface IDataReceiver
    {
        event EventHandler<Init> InitReceived;
        event EventHandler<Frame> FrameReceived;

        void Receive(ICommand command);
    }
}