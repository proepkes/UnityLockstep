using System.Collections.Generic;
using System.Linq;
using DesperateDevs.Utils;
using Entitas;                        
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems
{
    public class StoreNewOrChangedEntities : ReactiveSystem<GameEntity>
    {                                                                                                                             
        private readonly GameStateContext _gameStateContext;
        private readonly IStorageService _storageService;
        private readonly GameContext _gameContext;
        private int[] _componentIndices;
        private uint _internalIdCounter;
        
        public StoreNewOrChangedEntities(Contexts contexts, ServiceContainer services) : base(contexts.game)
        {
            _gameContext = contexts.game;
            _gameStateContext = contexts.gameState;
            _storageService = services.Get<IStorageService>();  
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            //Listen for changes on all components except IdReference
            _componentIndices = GameComponentsLookup.componentNames
                .Except(new[] { GameComponentsLookup.componentNames[GameComponentsLookup.IdReference] })
                .Select(componentName => (int)typeof(GameComponentsLookup)
                        .GetFields()
                        .First(info => info.Name == componentName)
                        .GetRawConstantValue())
                .ToArray();

            return context.CreateCollector(GameMatcher.AnyOf(_componentIndices).NoneOf(GameMatcher.IdReference));
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.hasId && !entity.hasIdReference;
        }

        protected override void Execute(List<GameEntity> entities)
        {                                                      
            var changedEntities = new List<uint>(entities.Count);
            foreach (var e in entities)
            {
                var backupEntity = _gameContext.CreateEntity();

                //Id is primary index => don't copy. Id is inside _componentIndices because we need to catch new entities that only have an Id-component and I'm too lazy to create a separate componentIndicesWithoutIdArray
                foreach (var index in _componentIndices.Where(i => i != GameComponentsLookup.Id && e.HasComponent(i)))
                {        
                    var component1 = e.GetComponent(index);
                    var component2 = backupEntity.CreateComponent(index, component1.GetType());
                    component1.CopyPublicMemberValues(component2);
                    backupEntity.AddComponent(index, component2);
                }

                backupEntity.AddIdReference(_internalIdCounter, e.id.value, _gameStateContext.tick.value);

                changedEntities.Add(_internalIdCounter);
                _internalIdCounter++;
            }

            _storageService.RegisterChange(_gameStateContext.tick.value, changedEntities);
        }
    }
}
