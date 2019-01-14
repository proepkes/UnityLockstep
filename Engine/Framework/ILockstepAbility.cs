namespace Lockstep.Framework
{
    public interface ILockstepAbility
    {
        void Simulate();

        void Setup(ILockstepAgent agent);
    }
}