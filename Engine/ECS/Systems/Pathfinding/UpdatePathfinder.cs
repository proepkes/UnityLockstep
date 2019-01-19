using Entitas;     

namespace ECS.Systems.Pathfinding
{
    public class UpdatePathfinder : IExecuteSystem
    {       
        //Moving entities are those who have a destination
        private readonly IGroup<GameEntity> _movingEntites;
        private readonly IPathfindingService _pathfindingService;

        public UpdatePathfinder(Contexts contexts, IPathfindingService pathfindingService)
        {
            _pathfindingService = pathfindingService;
            _movingEntites = contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.Movable, GameMatcher.Destination));
        }

        public void Execute()
        {   
            _pathfindingService?.UpdateAgents(_movingEntites.GetEntities());  
        }
    }
}
