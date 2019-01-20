using Entitas;

namespace ECS.Systems.Navigation
{
    public class UpdateNavigationService : IExecuteSystem
    {        
        private readonly INavigationService _navigationService;

        public UpdateNavigationService(Contexts contexts, INavigationService navigationService)
        {
            _navigationService = navigationService;                                                                                
        }

        public void Execute()
        {   
            _navigationService.UpdateAgents();  
        }
    }
}
