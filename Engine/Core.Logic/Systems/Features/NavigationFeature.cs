using Lockstep.Core.Logic.Interfaces.Services;
using Lockstep.Core.Logic.Systems.Game.Navigation;
using Lockstep.Core.Logic.Systems.Input;

namespace Lockstep.Core.Logic.Systems.Features
{
    public sealed class NavigationFeature : Feature
    {
        public NavigationFeature(Contexts contexts, ServiceContainer services)
        {
            Add(new OnNavigableDoRegisterAgent(contexts, services));
            Add(new ExecuteNavigationInput(contexts, services));
            Add(new NavigationTick(contexts, services));
            Add(new SyncAgentVelocity(contexts, services)); 
            //Add(new UpdateAgentPosition(contexts, navigationService));
        }
    }
}