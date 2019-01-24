using System.Collections.Generic;
using Entitas;

namespace ECS.Systems.Input
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
            return context.CreateCollector(InputMatcher.SpawnInputData);
        }

        protected override bool Filter(InputEntity entity)
        {
            return entity.hasSpawnInputData;
        }

        protected override void Execute(List<InputEntity> entities)
        {
            foreach (var entity in entities)
            {
                var e = _gameContext.CreateEntity();
                e.AddPosition(entity.spawnInputData.Position);

                _gameService.LoadEntity(e, entity.spawnInputData.EntityConfigId);
            }
        }   
    }
}
