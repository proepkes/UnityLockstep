using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesperateDevs.Utils;
using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems
{
    public class OnNewPredictionCreateSnapshot : ReactiveSystem<GameStateEntity>
    {
        private readonly IGroup<GameEntity> _activeEntities;
        private readonly GameStateContext _gameStateContext;
        private readonly List<GameEntity> _buffer = new List<GameEntity>();
        private readonly GameContext _gameContext;
        ISnapshotIndexService snapshotIndexService;

        public OnNewPredictionCreateSnapshot(Contexts contexts, Services services) : base(contexts.gameState)
        {
            _gameContext = contexts.game;
            _gameStateContext = contexts.gameState;
            snapshotIndexService = services.Get<ISnapshotIndexService>();

            _activeEntities = contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.Id, GameMatcher.OwnerId).NoneOf(GameMatcher.Shadow)); 
        }

        protected override ICollector<GameStateEntity> GetTrigger(IContext<GameStateEntity> context)
        {   
            //Create a snapshot as soon as prediction starts
            return context.CreateCollector(GameStateMatcher.Predicting.Added());
        }

        protected override bool Filter(GameStateEntity gameState)
        {
            return gameState.isPredicting;
        }

        protected override void Execute(List<GameStateEntity> entities)
        {
            snapshotIndexService.AddIndex(_gameStateContext.tick.value);

            Parallel.ForEach(_activeEntities.GetEntities(_buffer), e =>
            {
                var shadowEntity = _gameContext.CreateEntity();
                                                                                     
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
            });
        }
    }
}
