using Lockstep.Core.Data;

namespace Lockstep.Core.Interfaces
{
    public interface ISystems
    {                           
        uint CurrentTick { get; }        
        void Initialize();

        void Tick(ICommand[] input);

        void RevertToTick(uint tick);
    }
}