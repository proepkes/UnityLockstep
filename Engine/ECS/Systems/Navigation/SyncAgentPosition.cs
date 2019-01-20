using Entitas;

namespace ECS.Systems.Navigation
{
    public class SyncAgentPosition : IExecuteSystem
    {                                                       
        private readonly INavigationService _navigationService;
        private readonly GameContext _gameContext;

        public SyncAgentPosition(Contexts contexts, INavigationService navigationService)
        {
            _navigationService = navigationService;
            _gameContext = contexts.game;
        }

        public void Execute()
        {
            foreach (var entity in _gameContext.GetEntities())
            {
                entity.ReplacePosition(_navigationService.GetAgentPosition(entity.id.value));
            }                                    
        }      
    }
}     
