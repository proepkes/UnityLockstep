using Lockstep.Core.Data;

namespace Lockstep.Core.Interfaces
{
    public interface ISystems
    {
        void SetInput(ICommand[] input);       

        void Initialize();

        void Tick();                              
    }
}