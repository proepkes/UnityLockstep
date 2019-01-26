using Lockstep.Core.Data;

namespace Lockstep.Core.Interfaces
{
    public interface IFrameBuffer
    {
        uint Count { get; }
        uint ItemIndex { get; }
        uint Remaining { get; }

        void Insert(Frame item);
        Frame GetNext();
    }
}