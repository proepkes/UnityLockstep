using System.Collections.Generic;
using System.Linq;
using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems
{
    public class StoreNewEntities : ReactiveSystem<GameEntity>
    {                                                                                                                             
        private readonly GameStateContext _gameStateContext;
        private readonly IStorageService _storageService;

        public StoreNewEntities(Contexts contexts, ServiceContainer services) : base(contexts.game)
        {                                    
            _gameStateContext = contexts.gameState;
            _storageService = services.Get<IStorageService>();
        }      

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {                                                      
            return context.CreateCollector(GameMatcher.Id.Added());
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.hasId;
        }

        protected override void Execute(List<GameEntity> entities)
        {         
            _storageService.RegisterNew(_gameStateContext.tick.value, entities.Select(e => e.id.value).ToList());
        }
    }
}
