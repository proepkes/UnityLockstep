using BEPUutilities;

namespace Lockstep.Game.Interfaces
{
    public interface INavigationService : IService
    {
        void RegisterAgent(uint id, Vector2 position);

        void SetDestination(uint id, Vector2 destination);

    }
}