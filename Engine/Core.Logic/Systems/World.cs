using System.Collections.Generic;
using System.Linq;
using Entitas;
using Lockstep.Common.Logging;
using Lockstep.Core.Logic.Interfaces.Services;

namespace Lockstep.Core.Logic.Systems
{
    public class World
    {
        public Contexts Contexts { get; }

        public ServiceContainer Services { get; }

        public uint Tick => Contexts.gameState.tick.value;
                                                         
        private readonly SimulationSystems _systems;

        private readonly IViewService _view;     

        public World(Contexts contexts, ServiceContainer services, IEnumerable<byte> actorIds)
        {
            Contexts = contexts;
            Services = services;              

            _view = Services.Get<IViewService>();                

            foreach (var id in actorIds)
            {
                Contexts.actor.CreateEntity().AddId(id);
            }

            _systems = new SimulationSystems(Contexts, Services);
            _systems.Initialize();
        }

        public void Predict()
        {
            if (!Contexts.gameState.isPredicting)
            {
                Contexts.gameState.isPredicting = true;
            }

            Log.Trace(this, "Predict " + Contexts.gameState.tick.value);

            _systems.Execute();
            _systems.Cleanup();
        }

        public void Simulate()
        {
            if (Contexts.gameState.isPredicting)
            {
                Contexts.gameState.isPredicting = false;
            }

            Log.Trace(this, "Simulate " + Contexts.gameState.tick.value);

            _systems.Execute();
            _systems.Cleanup();                                                                                        
        }

        /// <summary>
        /// Reverts all changes that were done during or after the given tick
        /// </summary>
        public void RevertToTick(uint tick)
        {
            var snapshotIndices = Contexts.snapshot.GetEntities(SnapshotMatcher.Tick).Where(entity => entity.tick.value <= tick).Select(entity => entity.tick.value).ToList();
            var resultTick = snapshotIndices.Any() ? snapshotIndices.Max() : 0;


            Log.Trace(this, "Rolling back from " + resultTick + " to " + Contexts.gameState.tick.value);

            /*
             * ====================== Revert actors ======================
             * most importantly: the entity-count per actor gets reverted so the composite key (Id + ActorId) of GameEntities stays in sync
             */

            var backedUpActors = Contexts.actor.GetEntities(ActorMatcher.Backup).Where(e => e.backup.tick == resultTick);
            foreach (var backedUpActor in backedUpActors)
            {
                var target = Contexts.actor.GetEntityWithId(backedUpActor.backup.actorId);

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
                    Contexts.actor.GetEntityWithId(backedUpActor.backup.actorId),                      //Current Actor
                    true,                                                                             //Replace components
                    backedUpActor.GetComponentIndices().Except(new[] { ActorComponentsLookup.Backup }).ToArray()); //Copy everything except the backup-component
            }

            /*
            * ====================== Revert game-entities ======================      
            */

            var currentEntities = Contexts.game.GetEntities(GameMatcher.LocalId);
            var backupEntities = Contexts.game.GetEntities(GameMatcher.Backup).Where(e => e.backup.tick == resultTick).ToList();
            var backupEntityIds = backupEntities.Select(entity => entity.backup.localEntityId);

            //Entities that were created in the prediction have to be destroyed  
            var invalidEntities = currentEntities.Where(entity => !backupEntityIds.Contains(entity.localId.value)).ToList();
            foreach (var invalidEntity in invalidEntities)
            {
                //Here we have the actual entities, we can safely refer to them via the internal id
                _view.DeleteView(invalidEntity.localId.value);
                invalidEntity.Destroy();
            }

            foreach (var invalidBackupEntity in Contexts.game.GetEntities(GameMatcher.Backup).Where(e => e.backup.tick > resultTick))
            {                                                            
                invalidBackupEntity.Destroy();
            }

            foreach (var snapshotEntity in Contexts.snapshot.GetEntities(SnapshotMatcher.Tick).Where(e => e.tick.value > resultTick))
            {
                snapshotEntity.Destroy();
            }

            //Copy old state to the entity                                      
            foreach (var backupEntity in backupEntities)
            {
                var target = Contexts.game.GetEntityWithLocalId(backupEntity.backup.localEntityId);

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

            Contexts.gameState.ReplaceTick(resultTick);

            while (Tick <= tick)
            {
                Simulate();
            }
        }      
    }
}
