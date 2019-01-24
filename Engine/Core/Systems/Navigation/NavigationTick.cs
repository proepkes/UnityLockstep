using Entitas;

namespace ECS.Systems.Navigation
{
    public class NavigationTick : IExecuteSystem
    {        
        private readonly INavigationService _navigationService;

        public NavigationTick(Contexts contexts, INavigationService navigationService)
        {
            _navigationService = navigationService;                                                                       
        }

        public void Execute()
        {
            //All registered (navigable) entities have to be updated, because avoidance could move other entities aside
            _navigationService.Tick();  
        }
    }
}
