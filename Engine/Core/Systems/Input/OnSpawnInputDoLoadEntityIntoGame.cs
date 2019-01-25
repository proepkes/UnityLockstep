using System.Collections.Generic;
using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems.Input
{
    public class OnSpawnInputDoLoadEntityIntoGame : ReactiveSystem<InputEntity>
    {
        private readonly GameContext _gameContext;            
        private readonly IGameService _gameService;

        public OnSpawnInputDoLoadEntityIntoGame(Contexts contexts, IGameService gameService) : base(contexts.input)
        {
            _gameService = gameService; 
            _gameContext = contexts.game;
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context)
        {
            return context.CreateCollector(InputMatcher.AllOf(InputMatcher.EntityConfigId, InputMatcher.Coordinate));
        }

        protected override bool Filter(InputEntity entity)
        {
            return entity.hasEntityConfigId && entity.hasCoordinate;
        }

        protected override void Execute(List<InputEntity> entities)
        {
            foreach (var entity in entities)
            {
                var e = _gameContext.CreateEntity();
                e.AddPosition(entity.coordinate.value);

                _gameService.LoadEntity(e, entity.entityConfigId.value);
            }
        }   
    }
}
