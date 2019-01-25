namespace Lockstep.Core.Interfaces
{
    public interface ISystems
    {
        long HashCode { get; }

        void SetFrameBuffer(IFrameBuffer frameBuffer);

        void Initialize();

        void Tick();                              
    }
}