using System.Collections.Generic;
using System.Linq;
using Entitas;
using Lockstep.Core.Features;
using Lockstep.Core.Services;
using Lockstep.Core.Systems;
using Lockstep.Core.Systems.Actor;
using Lockstep.Core.Systems.Debugging;
using Lockstep.Core.Systems.GameState;

namespace Lockstep.Core.World
{
    class World : Feature, IWorld
    {
        public ServiceContainer Services { get; }

        public uint CurrentTick => _contexts.gameState.tick.value;

        public int ActiveEntities => _contexts.game.GetEntities(GameMatcher.LocalId).Length;

        private readonly Contexts _contexts;
        private readonly GameContext _gameContext;
        private readonly ActorContext _actorContext;

        private readonly IViewService _view;
        private readonly ILogService _logger;

        public World(Contexts contexts, ServiceContainer services)
        {
            _contexts = contexts;
            Services = services;
            
            _view = Services.Get<IViewService>();
            _logger = Services.Get<ILogService>();

            _gameContext = contexts.game;
            _actorContext = contexts.actor;
        }

        protected virtual void AddFeatures()
        {
            Add(new InitializeEntityCount(_contexts));

            Add(new OnNewPredictionCreateBackup(_contexts, Services));    

            Add(new InputFeature(_contexts, Services));

            Add(new VerifySelectionIdExists(_contexts, Services));

            Add(new NavigationFeature(_contexts, Services));

            Add(new GameEventSystems(_contexts));

            Add(new CalculateHashCode(_contexts, Services));

            Add(new RemoveNewFlag(_contexts));

            Add(new IncrementTick(_contexts));

            Add(new VerifyNoDuplicateBackups(_contexts, Services));
        }


        public void Initialize(IEnumerable<byte> allActorIds)
        {

            foreach (var id in allActorIds)
            {
                _contexts.actor.CreateEntity().AddId(id);
            }

            AddFeatures();

            base.Initialize();
        }      

        public InputEntity CreateInputEntity()
        {
            return _contexts.input.CreateEntity();
        }

        /// <summary>
        /// Reverts all changes that were done during or after the given tick
        /// </summary>
        public void RevertToTick(uint tick)
        {

            //Get the actual tick that we have a snapshot for
            var resultTick = Services.Get<ISnapshotIndexService>().GetFirstIndexBefore(tick);

            _logger.Warn(() => "Rolling back from " + resultTick + " to " + CurrentTick);

            /*
             * ====================== Revert actors ======================
             * most importantly: the entity-count per actor gets reverted so the composite key (Id + ActorId) of GameEntities stays in sync
             */

            var backedUpActors = _actorContext.GetEntities(ActorMatcher.Backup).Where(e => e.backup.tick == resultTick);
            foreach (var backedUpActor in backedUpActors)
            {
                var target = _actorContext.GetEntityWithId(backedUpActor.backup.actorId);

                //CopyTo does NOT remove additional existing components, so remove them first
                var additionalComponentIndices = target.GetComponentIndices().Except(
                    backedUpActor.GetComponentIndices()
                        .Except(new[] { ActorComponentsLookup.Backup })
                        .Concat(new[] { ActorComponentsLookup.Id }));

                foreach (var index in additionalComponentIndices)
                {
                    target.RemoveComponent(index);
                }

                backedUpActor.CopyTo(
                    _actorContext.GetEntityWithId(backedUpActor.backup.actorId),                                   //Current Actor
                    true,                                                                             //Replace components
                    backedUpActor.GetComponentIndices().Except(new[] { ActorComponentsLookup.Backup }).ToArray()); //Copy everything except the backup-component
            }

            /*
            * ====================== Revert game-entities ======================      
            */

            var currentEntities = _gameContext.GetEntities(GameMatcher.LocalId);
            var backupEntities = _gameContext.GetEntities(GameMatcher.Backup).Where(e => e.backup.tick == resultTick).ToList();
            var backupEntityIds = backupEntities.Select(entity => entity.backup.localEntityId);

            //Entities that were created in the prediction have to be destroyed  
            var invalidEntities = currentEntities.Where(entity => !backupEntityIds.Contains(entity.localId.value)).ToList();
            foreach (var invalidEntity in invalidEntities)
            {
                //Here we have the actual entities, we can safely refer to them via the internal id
                _view.DeleteView(invalidEntity.localId.value);
                invalidEntity.Destroy();
            }

            foreach (var invalidBackupEntity in _gameContext.GetEntities(GameMatcher.Backup).Where(e => e.backup.tick > resultTick))
            {
                Services.Get<ISnapshotIndexService>().RemoveIndex(invalidBackupEntity.backup.tick);
                invalidBackupEntity.Destroy();
            }

            //Copy old state to the entity                                      
            foreach (var backupEntity in backupEntities)
            {
                var target = _gameContext.GetEntityWithLocalId(backupEntity.backup.localEntityId);

                //CopyTo does NOT remove additional existing components, so remove them first
                var additionalComponentIndices = target.GetComponentIndices().Except(
                        backupEntity.GetComponentIndices()
                            .Except(new[] { GameComponentsLookup.Backup })
                            .Concat(new[] { GameComponentsLookup.Id, GameComponentsLookup.ActorId, GameComponentsLookup.LocalId }));
                foreach (var index in additionalComponentIndices)
                {
                    target.RemoveComponent(index);
                }

                backupEntity.CopyTo(target, true, backupEntity.GetComponentIndices().Except(new[] { GameComponentsLookup.Backup }).ToArray());
            }

            //TODO: restore locally destroyed entities   

            _contexts.gameState.ReplaceTick(resultTick);
        }

        public void Predict()
        {
            if (!_contexts.gameState.isPredicting)
            {
                _contexts.gameState.isPredicting = true;
            }

            _logger.Trace(() => "Predict " + CurrentTick);

            Execute();
            Cleanup();
        }

        public void Simulate()
        {
            if (_contexts.gameState.isPredicting)
            {
                _contexts.gameState.isPredicting = false;
            }

            _logger.Trace(() => "Simulate " + CurrentTick);

            Execute();
            Cleanup();

            Services.Get<IDebugService>().Register(_contexts.gameState.tick.value, _contexts.gameState.hashCode.value);
        }
    }
}