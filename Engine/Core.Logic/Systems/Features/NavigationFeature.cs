using Lockstep.Core.Logic.Interfaces.Services;
using Lockstep.Core.Logic.Systems.Game.Navigation;
using Lockstep.Core.Logic.Systems.Input;

namespace Lockstep.Core.Logic.Systems.Features
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