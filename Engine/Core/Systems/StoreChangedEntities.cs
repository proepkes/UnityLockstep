using System.Collections.Generic;
using System.Linq;
using DesperateDevs.Utils;
using Entitas;                        
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems
{
    public class StoreChangedEntities : ReactiveSystem<GameEntity>
    {                                                                                                                             
        private readonly GameStateContext _gameStateContext;
        private readonly IStorageService _storageService;
        private readonly GameContext _gameContext;
        private int[] _componentIndices;  
        
        public StoreChangedEntities(Contexts contexts, ServiceContainer services) : base(contexts.game)
        {
            _gameContext = contexts.game;
            _gameStateContext = contexts.gameState;
            _storageService = services.Get<IStorageService>();  
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            //Listen for changes on all components except Id
            _componentIndices = GameComponentsLookup.componentNames
                .Except(new[] { GameComponentsLookup.componentNames[GameComponentsLookup.Id], GameComponentsLookup.componentNames[GameComponentsLookup.IdReference] })
                .Select(componentName => (int)typeof(GameComponentsLookup)
                        .GetFields()
                        .First(info => info.Name == componentName)
                        .GetRawConstantValue())
                .ToArray();

            return context.CreateCollector(GameMatcher.AnyOf(_componentIndices));
        }

        protected override bool Filter(GameEntity entity)
        {
            return !entity.hasIdReference;
        }

        protected override void Execute(List<GameEntity> entities)
        {                                                      
            var changedEntities = new List<GameEntity>(entities.Count);
            foreach (var e in entities)
            {
                var backupEntity = _gameContext.CreateEntity();
                backupEntity.AddIdReference(e.id.value);

                foreach (var index in _componentIndices)
                {                            
                    if (e.HasComponent(index))
                    {
                        var component1 = e.GetComponent(index);
                        var component2 = backupEntity.CreateComponent(index, component1.GetType());
                        component1.CopyPublicMemberValues((object)component2);
                        backupEntity.AddComponent(index, component2);
                    }
                }                         
                changedEntities.Add(backupEntity);
            }

            _storageService.RegisterChange(_gameStateContext.tick.value, changedEntities);
        }
    }
}
