using Entitas;

namespace ECS.Systems.Navigation
{
    public class SyncAgentPosition : IExecuteSystem
    {                                                       
        private readonly INavigationService _navigationService;   
        private readonly IGroup<GameEntity> _navigableEntities;

        public SyncAgentPosition(Contexts contexts, INavigationService navigationService)
        {
            _navigationService = navigationService;   
            _navigableEntities = contexts.game.GetGroup(GameMatcher.Navigable);
        }

        public void Execute()
        {
            foreach (var entity in _navigableEntities.GetEntities())
            {
                entity.ReplacePosition(_navigationService.GetAgentPosition(entity.id.value));
            }                                    
        }      
    }
}     
