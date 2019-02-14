using System.Collections.Generic;
using System.Linq;
using DesperateDevs.Utils;
using Entitas;
using Lockstep.Common.Logging;                       

namespace Lockstep.Core.Logic.Systems.GameState
{
    public class OnNewPredictionCreateSnapshot : ReactiveSystem<GameStateEntity>
    {
        private readonly GameContext _gameContext;
        private readonly ActorContext _actorContext;
        private readonly SnapshotContext _snapshotContext;
        private readonly GameStateContext _gameStateContext;


        private readonly IGroup<ActorEntity> _activeActors;
        private readonly IGroup<GameEntity> _activeEntities;

        public OnNewPredictionCreateSnapshot(Contexts contexts) : base(contexts.gameState)
        {
            _gameContext = contexts.game;
            _actorContext = contexts.actor;
            _snapshotContext = contexts.snapshot;
            _gameStateContext = contexts.gameState;                                

            _activeActors = contexts.actor.GetGroup(ActorMatcher.Id);
            _activeEntities = contexts.game.GetGroup(GameMatcher.LocalId);
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
            var currentTick = _gameStateContext.tick.value;

            //Register the tick for which a snapshot is created
            _snapshotContext.CreateEntity().AddTick(currentTick);     

            foreach (var e in _activeEntities)
            {
                var shadowEntity = _gameContext.CreateEntity();

                //LocalId is primary index => don't copy; everything else has to be copied in case a destroyed entity has to be recovered
                foreach (var index in e.GetComponentIndices().Except(new[] { GameComponentsLookup.LocalId }))
                {
                    var component1 = e.GetComponent(index);
                    var component2 = shadowEntity.CreateComponent(index, component1.GetType());
                    component1.CopyPublicMemberValues(component2);
                    shadowEntity.AddComponent(index, component2);
                }

                shadowEntity.AddBackup(e.localId.value, currentTick);
            }

            foreach (var actor in _activeActors)
            {
                var shadowActor = _actorContext.CreateEntity();

                //Id is primary index => don't copy
                foreach (var index in actor.GetComponentIndices().Except(new[] { ActorComponentsLookup.Id }))
                {
                    var actorComponent = actor.GetComponent(index);
                    var backupComponent = shadowActor.CreateComponent(index, actorComponent.GetType());
                    actorComponent.CopyPublicMemberValues(backupComponent);
                    shadowActor.AddComponent(index, backupComponent);
                }

                shadowActor.AddBackup(actor.id.value, currentTick);
            }

            Log.Trace(this, "New snapshot for " + currentTick + "(" + _activeActors.count + " actors, " + _activeEntities.count + " entities)");
        }
    }
}
