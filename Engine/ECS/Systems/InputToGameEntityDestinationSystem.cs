using System;
using System.Collections.Generic;
using Entitas;

namespace ECS.Systems
{
    class InputToGameEntityDestinationSystem : ReactiveSystem<InputEntity>
    {
        private readonly GameContext _gameContext;

        public InputToGameEntityDestinationSystem(Contexts contexts) : base(contexts.input)
        {
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context)
        {
            return context.CreateCollector(InputMatcher.NavigationInput.Added());
        }

        protected override bool Filter(InputEntity entity)
        {
            return entity.hasGameEntityIds && entity.hasInputPosition;
        }

        protected override void Execute(List<InputEntity> entities)
        {
            foreach (InputEntity e in entities)
            {
                //TODO: Add FilterSystem to only iterate over gameEntites that are controlled by the player who sent the command
                foreach (var entityId in e.gameEntityIds.value)
                {
                    _gameContext.GetEntityWithId(entityId).ReplaceDestination(e.inputPosition.value);
                }
            }
        }
    }
}
