using System;                     
using Lockstep.Core.Data;

namespace Lockstep.Core.Interfaces
{
    public interface ICommandBuffer
    {
        event Action<byte, long, ICommand[]> Inserted;

        long Count { get; }
        long ItemIndex { get; }
        long Remaining { get; }

        void Insert(byte commanderId, long frameNumber, ICommand[] commands);

        ICommand[] GetNext();
    }
}