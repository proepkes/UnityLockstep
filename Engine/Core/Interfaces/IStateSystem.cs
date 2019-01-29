namespace Lockstep.Core.Interfaces
{
    public interface IStateSystem
    {
        void RevertFromTick(uint tick);
    }
}