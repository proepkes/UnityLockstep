using ECS.Systems.Navigation;

namespace ECS.Features
{
    public sealed class NavigationFeature : Feature
    {
        public NavigationFeature(Contexts contexts, ServiceContainer serviceContainer)
        {
            Add(new OnGameEntityMovableRegisterToPathfinder(contexts, serviceContainer.Get<INavigationService>())); 
            Add(new OnInputSetDestination(contexts, serviceContainer.Get<INavigationService>(), serviceContainer.Get<ILogger>()));
            Add(new CheckDestinationReached(contexts));   
            Add(new UpdateNavigationService(contexts, serviceContainer.Get<INavigationService>()));
            Add(new SyncAgentPosition(contexts, serviceContainer.Get<INavigationService>()));   
        }
    }
}
