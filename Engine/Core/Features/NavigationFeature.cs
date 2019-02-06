using Lockstep.Core.Services;
using Lockstep.Core.Systems.Input;
using Lockstep.Core.Systems.Navigation;

namespace Lockstep.Core.Features
{
    public sealed class NavigationFeature : Feature
    {
        public NavigationFeature(Contexts contexts, ServiceContainer serviceContainer)
        {
            var navigationService = serviceContainer.Get<INavigationService>();

            //Add(new OnNavigableDoRegisterAgent(contexts, navigationService));
            Add(new ExecuteNavigationInput(contexts, serviceContainer));     
            Add(new NavigationTick(contexts, navigationService));
            //Add(new SyncAgentVelocity(contexts, navigationService)); 
            //Add(new UpdateAgentPosition(contexts, navigationService));
        }
    }
}
