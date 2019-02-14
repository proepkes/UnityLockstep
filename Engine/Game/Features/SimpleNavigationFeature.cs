using Lockstep.Game.Features.Navigation.Simple;

namespace Lockstep.Game.Features
{
    sealed class SimpleNavigationFeature : Feature
    {
        public SimpleNavigationFeature(Contexts contexts, ServiceContainer services) : base("SimpleNavigation")
        {
            Add(new NavigationTick(contexts, services));

        }
    }
}
