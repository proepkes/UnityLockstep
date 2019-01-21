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
            _navigationService.UpdateAgents();  
        }
    }
}
