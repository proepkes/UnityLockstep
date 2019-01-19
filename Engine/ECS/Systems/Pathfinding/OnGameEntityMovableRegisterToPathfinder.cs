using System.Collections.Generic;    
using Entitas;

namespace ECS.Systems.Pathfinding
{
    public class OnGameEntityMovableRegisterToPathfinder : ReactiveSystem<GameEntity>
    {
        private readonly IPathfindingService _pathfindingService;

        public OnGameEntityMovableRegisterToPathfinder(Contexts contexts, IPathfindingService pathfindingService) : base(contexts.game)
        {
            _pathfindingService = pathfindingService;
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.Movable);
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.isMovable;
        }

        protected override void Execute(List<GameEntity> entities)
        {
            foreach (var entity in entities)
            {
                _pathfindingService.AddAgent(entity, entity.position.value); 
            } 
        }
    }
}
