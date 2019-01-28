namespace Lockstep.Core.Interfaces
{
    public interface IStateSystem
    {
        void RevertToTick(uint tick);
    }
}