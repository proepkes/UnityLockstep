using Lockstep.Game.Features.Navigation.RVO;

namespace Lockstep.Game.Features
{
    sealed class RVONavigationFeature : Feature
    {
        public RVONavigationFeature(Contexts contexts, ServiceContainer services) : base("RVONavigation")
        {
            Add(new ComputeAgentPreferredVelocity(contexts, services));
            Add(new ComputeAgentVelocity(contexts, services));
            Add(new UpdateAgent(contexts, services));

            //Add(new NavigationTick(contexts, services));
        }
    }
}
