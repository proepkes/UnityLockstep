using Lockstep.Core.Services;
using Lockstep.Game.Services;
using Lockstep.Game.Systems.Navigation;

namespace Lockstep.Game.Features
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
