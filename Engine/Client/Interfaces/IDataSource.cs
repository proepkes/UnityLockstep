using System;
using Lockstep.Core.Data;

namespace Lockstep.Core.Interfaces
{
    public interface IDataSource
    {
        event EventHandler InitReceived;
        event EventHandler<Frame> FrameReceived;

        void Receive(ICommand command);
    }
}