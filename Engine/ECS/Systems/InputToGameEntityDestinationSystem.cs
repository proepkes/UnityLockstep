using System.Collections.Generic;
using Entitas;

namespace ECS.Systems
{
    class InputToGameEntityDestinationSystem : ReactiveSystem<InputEntity>
    {
        private readonly GameContext _gameContext;

        public InputToGameEntityDestinationSystem(Contexts contexts) : base(contexts.input)
        {
            _gameContext = contexts.game;
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context)
        {
            return context.CreateCollector(InputMatcher.NavigationInput);
        }

        protected override bool Filter(InputEntity entity)
        {
            return entity.hasGameEntityIds && entity.hasMousePosition;
        }

        protected override void Execute(List<InputEntity> entities)
        {
            foreach (InputEntity e in entities)
            {
                //TODO: Add PlayerControlledSystem to only iterate over gameEntites that are controlled by the player who sent the serializedInput
                foreach (var entityId in e.gameEntityIds.value)
                {
                    _gameContext.GetEntityWithId(entityId).ReplaceDestination(e.mousePosition.value);
                }
            }
        }
    }
}
