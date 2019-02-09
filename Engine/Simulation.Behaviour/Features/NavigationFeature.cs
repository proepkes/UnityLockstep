using Lockstep.Core.Services;
using Simulation.Behaviour.Game.Navigation;
using Simulation.Behaviour.Input;
using Simulation.Behaviour.Services;

namespace Simulation.Behaviour.Features
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