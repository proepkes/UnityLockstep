using System.Collections.Generic;
using System.Linq;
using DesperateDevs.Utils;
using Entitas;                   

namespace Lockstep.Core.Systems
{
    public class StoreNewOrChangedEntities : ReactiveSystem<GameEntity>
    {                                                                                                                             
        private readonly GameStateContext _gameStateContext;   
        private readonly GameContext _gameContext;
        private int[] _componentIndices;    
        
        public StoreNewOrChangedEntities(Contexts contexts) : base(contexts.game)
        {
            _gameContext = contexts.game;
            _gameStateContext = contexts.gameState;                
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            //Listen for changes on all components except IdReference
            _componentIndices = GameComponentsLookup.componentNames
                .Except(new[] { GameComponentsLookup.componentNames[GameComponentsLookup.Shadow] })
                .Select(componentName => (int)typeof(GameComponentsLookup)
                        .GetFields()
                        .First(info => info.Name == componentName)
                        .GetRawConstantValue())
                .ToArray();

            return context.CreateCollector(GameMatcher.AnyOf(_componentIndices).NoneOf(GameMatcher.Shadow));
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.hasId && !entity.isShadow;
        }

        protected override void Execute(List<GameEntity> entities)
        {                                                           
            foreach (var e in entities)
            {
                var shadowEntity = _gameContext.CreateEntity();

                //TODO: find out a way to only copy components that actually changed
                //LocalId is primary index => don't copy
                foreach (var index in e.GetComponentIndices().Where(i => i != GameComponentsLookup.LocalId))
                {      
                    var component1 = e.GetComponent(index);
                    var component2 = shadowEntity.CreateComponent(index, component1.GetType());
                    component1.CopyPublicMemberValues(component2);
                    shadowEntity.AddComponent(index, component2);
                }

                shadowEntity.isShadow = true;
                shadowEntity.AddTick(_gameStateContext.tick.value);
            }                                                                              
        }
    }
}
