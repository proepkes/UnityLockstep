using Lockstep.Core.Data;

namespace Lockstep.Core.Interfaces
{
    public interface ICommandBuffer
    {
        ulong Count { get; }
        ulong ItemIndex { get; }
        ulong Remaining { get; }

        void Insert(ulong frameNumber, ICommand command);

        ICommand[] GetNext();
    }
}