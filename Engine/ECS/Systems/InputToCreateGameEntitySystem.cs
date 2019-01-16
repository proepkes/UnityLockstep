using System.Collections.Generic;
using Entitas;

namespace ECS.Systems
{
    class InputToCreateGameEntitySystem : ReactiveSystem<InputEntity>
    {                                                   
        private readonly GameContext _gameContext;

        public InputToCreateGameEntitySystem(Contexts contexts) : base(contexts.input)
        {                                 
            _gameContext = contexts.game;
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context)
        {
            return context.CreateCollector(InputMatcher.SpawnInput);
        }

        protected override bool Filter(InputEntity entity)
        {
            return entity.hasSpawnInput;
        }

        protected override void Execute(List<InputEntity> entities)
        {
            foreach (InputEntity e in entities)
            {             
                var playerEntity = _gameContext.CreateEntity();
                playerEntity.AddAsset(e.spawnInput.assetName); 
            }
        }
    }
}
