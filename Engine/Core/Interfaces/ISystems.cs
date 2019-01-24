namespace Lockstep.Core.Interfaces
{
    public interface ISystems
    {
        IFrameDataSource DataSource { get; }

        void Initialize();

        void Tick();
    }
}