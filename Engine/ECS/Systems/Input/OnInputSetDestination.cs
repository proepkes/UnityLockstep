using System.Collections.Generic;
using Entitas;

namespace ECS.Systems.Input
{
    public class OnInputSetDestination : ReactiveSystem<InputEntity>
    {
        private readonly ILogger _logger;
        private readonly GameContext _gameContext;

        public OnInputSetDestination(Contexts contexts, ILogger logger) : base(contexts.input)
        {
            _logger = logger;
            _gameContext = contexts.game;
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context)
        {
            return context.CreateCollector(InputMatcher.NavigationInput);
        }

        protected override bool Filter(InputEntity entity)
        {
            return entity.hasGameEntityIds && entity.hasInputPosition;
        }

        protected override void Execute(List<InputEntity> entities)
        {
            foreach (InputEntity e in entities)
            {
                //TODO: Add PlayerControlledSystem to only iterate over gameEntites that are controlled by the player who sent the serializedInput
                foreach (var entityId in e.gameEntityIds.value)
                {
                    var gameEntity = _gameContext.GetEntityWithId(entityId);
                    if (!gameEntity.isMovable)
                    {
                        _logger?.Warn("Adding a destination to a non-movable entity");
                    }

                    gameEntity.ReplaceDestination(e.inputPosition.value);
                }
            }
        }
    }
}
