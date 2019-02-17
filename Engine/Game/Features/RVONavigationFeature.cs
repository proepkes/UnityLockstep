using Lockstep.Game.Features.Navigation.RVO;
using Lockstep.Game.Features.Navigation.RVO.Algorithm;

namespace Lockstep.Game.Features
{
    sealed class RVONavigationFeature : Feature
    {
        public RVONavigationFeature(Contexts contexts, ServiceContainer services) : base("RVONavigation")
        {
            Simulator.Instance.setTimeStep(0.5m);
            Add(new ComputeAgentPreferredVelocity(contexts, services));
            Add(new ComputeAgentVelocity(contexts, services));
            Add(new UpdateAgent(contexts, services));

            //Add(new NavigationTick(contexts, services));
        }
    }
}
