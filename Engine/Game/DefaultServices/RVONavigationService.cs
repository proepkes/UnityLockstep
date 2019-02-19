using BEPUutilities;
using Lockstep.Game.Features.Navigation.RVO.Algorithm;
using Lockstep.Game.Interfaces;

namespace Lockstep.Game.DefaultServices
{
    public class RVONavigationService : INavigationService
    {
        public void RegisterAgent(uint id, Vector2 position)
        {
            Simulator.Instance.addAgent(id, position);
        }

        public void SetDestination(uint id, Vector2 destination)
        {
            Simulator.Instance.agents_[id].Destination = destination;
        }
    }
}
