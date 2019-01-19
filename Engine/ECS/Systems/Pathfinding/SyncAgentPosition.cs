using Entitas;

namespace ECS.Systems.Pathfinding
{
    public class SyncAgentPosition : IExecuteSystem
    {                                         
        private readonly IGroup<GameEntity> _movingEntites;
        private readonly IPathfindingService _pathfindingService;

        public SyncAgentPosition(Contexts contexts, IPathfindingService pathfindingService)
        {
            _pathfindingService = pathfindingService;
            _movingEntites = contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.Movable, GameMatcher.Destination));
        }

        public void Execute()
        {
            foreach (var entity in _movingEntites.GetEntities())
            {
                entity.ReplacePosition(_pathfindingService.GetAgentPosition(entity.id.value));
            }                                    
        }      
    }
}     
