using Lockstep.Core.Data;

namespace Lockstep.Core.Interfaces
{
    public interface ISystems
    {
        long HashCode { get; }

        void SetFrame(Frame frame);

        void Initialize();

        void Tick();                              
    }
}