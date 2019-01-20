using Entitas;

namespace ECS.Systems.Navigation
{
    public class TickNavigationService : IExecuteSystem
    {        
        private readonly INavigationService _navigationService;

        public TickNavigationService(Contexts contexts, INavigationService navigationService)
        {
            _navigationService = navigationService;                                                                                
        }

        public void Execute()
        {   
            _navigationService.UpdateAgents();  
        }
    }
}
