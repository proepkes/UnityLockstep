using System.Collections.Generic;
using Entitas;

namespace ECS.Systems.Input
{
    public class OnInputCreateGameEntity : ReactiveSystem<InputEntity>
    {
        private readonly IHashService _hashService;
        private readonly GameContext _gameContext;

        public OnInputCreateGameEntity(Contexts contexts, IHashService hashService) : base(contexts.input)
        {
            _hashService = hashService;
            _gameContext = contexts.game;
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context)
        {
            return context.CreateCollector(InputMatcher.SpawnInput);
        }

        protected override bool Filter(InputEntity entity)
        {
            return entity.hasSpawnInput && entity.hasInputPosition;
        }

        protected override void Execute(List<InputEntity> entities)
        {
            foreach (var e in entities)
            {             
                var gameEntity = _gameContext.CreateEntity();
                gameEntity.AddAsset(e.spawnInput.assetName); 
                gameEntity.AddPosition(e.inputPosition.value);
                gameEntity.isMovable = e.spawnInput.movable;  

                gameEntity.AddHashCode(_hashService.GetHashCode(gameEntity));
            }
        }
    }
}
