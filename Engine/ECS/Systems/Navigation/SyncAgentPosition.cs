using Entitas;

namespace ECS.Systems.Navigation
{
    public class SyncAgentPosition : IExecuteSystem
    {                                         
        private readonly IGroup<GameEntity> _movingEntites;
        private readonly INavigationService _navigationService;

        public SyncAgentPosition(Contexts contexts, INavigationService navigationService)
        {
            _navigationService = navigationService;
            _movingEntites = contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.Movable, GameMatcher.Destination));
        }

        public void Execute()
        {
            foreach (var entity in _movingEntites.GetEntities())
            {
                entity.ReplacePosition(_navigationService.GetAgentPosition(entity.id.value));
            }                                    
        }      
    }
}     
