using Lockstep.Core.Interfaces;
using Lockstep.Core.Systems.Navigation;

namespace Lockstep.Core.Features
{
    public sealed class NavigationFeature : Feature
    {
        public NavigationFeature(Contexts contexts, Services services)
        {
            var navigationService = services.Get<INavigationService>();

            //Add(new OnNavigableDoRegisterAgent(contexts, navigationService));
            Add(new OnNavigationInputDoSetDestination(contexts, services));     
            Add(new NavigationTick(contexts, navigationService));
            //Add(new SyncAgentVelocity(contexts, navigationService)); 
            //Add(new UpdateAgentPosition(contexts, navigationService));
        }
    }
}
