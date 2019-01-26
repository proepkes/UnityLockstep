using Lockstep.Core.Interfaces;
using Lockstep.Core.Systems.Navigation;

namespace Lockstep.Core.Features
{
    public sealed class NavigationFeature : Feature
    {
        public NavigationFeature(Contexts contexts, ServiceContainer serviceContainer)
        {
            var navigationService = serviceContainer.Get<INavigationService>();

            Add(new OnNavigableDoRegisterAgent(contexts, navigationService));
            Add(new OnNavigationInputDoSetDestination(contexts, navigationService));     
            Add(new NavigationTick(contexts, navigationService));
            Add(new SyncAgentVelocity(contexts, navigationService)); 
            Add(new UpdateAgentPosition(contexts, navigationService));
        }
    }
}
