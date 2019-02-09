using System.Collections.Generic;
using System.Linq;
using Entitas;
using Lockstep.Core.Services;
using Simulation.Behaviour;
using Simulation.Behaviour.Services;

namespace Lockstep.Game
{
    public class World
    {
        public Contexts Contexts { get; }
        public ServiceContainer Services { get; }

        public uint Tick => Contexts.gameState.tick.value;

        private readonly GameContext _gameContext;
        private readonly ActorContext _actorContext;
        private readonly GameStateContext _gameState;

        private readonly IViewService _view;
        private readonly ILogService _logger;
        private readonly ISnapshotIndexService _snapshots;

        private readonly SimulationSystems _systems;

        public World(Contexts contexts, ServiceContainer services, IEnumerable<byte> actorIds)
        {
            Contexts = contexts;
            Services = services;

            _gameContext = Contexts.game;
            _actorContext = Contexts.actor;
            _gameState = Contexts.gameState;

            _view = Services.Get<IViewService>();
            _logger = Services.Get<ILogService>();
            _snapshots = Services.Get<ISnapshotIndexService>();

            foreach (var id in actorIds)
            {
                _actorContext.CreateEntity().AddId(id);
            }

            _systems = new SimulationSystems(Contexts, Services);
            _systems.Initialize();
        }

        public void Predict()
        {
            if (!_gameState.isPredicting)
            {
                _gameState.isPredicting = true;
            }

            _logger.Trace(() => "Predict " + _gameState.tick.value);

            _systems.Execute();
            _systems.Cleanup();
        }

        public void Simulate()
        {
            if (_gameState.isPredicting)
            {
                _gameState.isPredicting = false;
            }

            _logger.Trace(() => "Simulate " + _gameState.tick.value);

            _systems.Execute();
            _systems.Cleanup();

            Services.Get<IDebugService>().Register(_gameState.tick.value, _gameState.hashCode.value);
        }

        /// <summary>
        /// Reverts all changes that were done during or after the given tick
        /// </summary>
        public void RevertToTick(uint tick)
        {

            _logger.Warn(() => "Rolling back from " + tick + " to " + _gameState.tick.value);

            /*
             * ====================== Revert actors ======================
             * most importantly: the entity-count per actor gets reverted so the composite key (Id + ActorId) of GameEntities stays in sync
             */

            var backedUpActors = _actorContext.GetEntities(ActorMatcher.Backup).Where(e => e.backup.tick == tick);
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
                    _actorContext.GetEntityWithId(backedUpActor.backup.actorId),                      //Current Actor
                    true,                                                                             //Replace components
                    backedUpActor.GetComponentIndices().Except(new[] { ActorComponentsLookup.Backup }).ToArray()); //Copy everything except the backup-component
            }

            /*
            * ====================== Revert game-entities ======================      
            */

            var currentEntities = _gameContext.GetEntities(GameMatcher.LocalId);
            var backupEntities = _gameContext.GetEntities(GameMatcher.Backup).Where(e => e.backup.tick == tick).ToList();
            var backupEntityIds = backupEntities.Select(entity => entity.backup.localEntityId);

            //Entities that were created in the prediction have to be destroyed  
            var invalidEntities = currentEntities.Where(entity => !backupEntityIds.Contains(entity.localId.value)).ToList();
            foreach (var invalidEntity in invalidEntities)
            {
                //Here we have the actual entities, we can safely refer to them via the internal id
                _view.DeleteView(invalidEntity.localId.value);
                invalidEntity.Destroy();
            }

            foreach (var invalidBackupEntity in _gameContext.GetEntities(GameMatcher.Backup).Where(e => e.backup.tick > tick))
            {
                _snapshots.RemoveIndex(invalidBackupEntity.backup.tick);
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

            _gameState.ReplaceTick(tick);
        }      
    }
}
