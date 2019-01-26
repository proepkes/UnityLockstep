using Lockstep.Core.Data;

namespace Lockstep.Core.Interfaces
{
    public interface ICommandBuffer
    {
        long Count { get; }
        long ItemIndex { get; }
        long Remaining { get; }

        void Insert(long frameNumber, ICommand command);

        ICommand[] GetNext();
    }
}