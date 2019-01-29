namespace Lockstep.Core.Interfaces
{
    public interface ISystems
    {
        ServiceContainer Services { get; }

        uint CurrentTick { get; }        
        void Initialize();

        void Tick(ICommand[] input);

        void RevertFromTick(uint tick);
    }
}