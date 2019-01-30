namespace Lockstep.Core.Interfaces
{
    public interface ITickable
    {
        int EntitiesInCurrentTick { get; }  

        uint CurrentTick { get; }  
        
        void Initialize();

        void Tick(ICommand[] input);

        void RevertToTick(uint tick);
    }
}