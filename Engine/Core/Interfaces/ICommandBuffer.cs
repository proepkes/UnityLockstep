using System;
using Lockstep.Core.Data;

namespace Lockstep.Core.Interfaces
{
    public interface ICommandBuffer
    {
        event Action<long, ICommand> Inserted;

        long Count { get; }
        long ItemIndex { get; }
        long Remaining { get; }

        void Insert(long frameNumber, ICommand command);

        ICommand[] GetNext();
    }
}