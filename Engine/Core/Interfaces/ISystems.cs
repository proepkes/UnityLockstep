namespace Lockstep.Core.Interfaces
{
    public interface ISystems
    {
        long HashCode { get; }

        IFrameDataSource DataSource { get; }

        void Initialize();

        void Tick();
    }
}