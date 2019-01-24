using ECS.Systems.Navigation;

namespace ECS.Features
{
    public sealed class NavigationFeature : Feature
    {
        public NavigationFeature(Contexts contexts, ServiceContainer serviceContainer)
        {
            var navigationService = serviceContainer.Get<INavigationService>();

            Add(new OnNavigableDoRegisterAgent(contexts, navigationService));
            Add(new OnNavigationInputDoSetDestination(contexts, navigationService));     
            Add(new NavigationTick(contexts, navigationService));
            Add(new SyncAgentPosition(contexts, navigationService));
        }
    }
}
