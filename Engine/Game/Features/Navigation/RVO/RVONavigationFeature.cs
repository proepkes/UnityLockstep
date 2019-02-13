using Entitas;

namespace Lockstep.Game.Features.Navigation.RVO
{
    sealed class RVONavigationFeature : Feature
    {
        public RVONavigationFeature(Contexts contexts, ServiceContainer services) : base("RVONavigation")
        {
            //Add(new UpdatePreferredVelocity(contexts, services));

            Add(new NavigationTick(contexts, services));
        }
    }
}
