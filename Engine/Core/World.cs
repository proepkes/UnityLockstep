using System.Collections.Generic;
using System.Linq;
using Entitas;                            
using Lockstep.Core.Features;
using Lockstep.Core.Interfaces;
using Lockstep.Core.Systems;
using Lockstep.Core.Systems.Debugging;
using Lockstep.Core.Systems.GameState;

namespace Lockstep.Core
{
    public sealed class World : Feature, IWorld
    {
        private Contexts Contexts { get; }

        public GameLog GameLog { get; } = new GameLog();

        public Services Services { get; }

        public uint CurrentTick => Contexts.gameState.tick.value;

        public int EntitiesInCurrentTick => Contexts.game.GetEntities(GameMatcher.LocalId).Length;

        private readonly IViewService _view;
        private readonly GameContext _gameContext;
        private readonly INavigationService _navigation;      
        private readonly ActorContext _actorContext;
                                                                           

        public World(Contexts contexts, params IService[] additionalServices)
        {
            Contexts = contexts;

            Services = new Services();
            foreach (var service in additionalServices)
            {
                Services.Register(service);
            }

            _view = Services.Get<IViewService>();
            _navigation = Services.Get<INavigationService>();       

            _gameContext = contexts.game;
            _actorContext = contexts.actor;

            AddFeatures(contexts);
        }

        private void AddFeatures(Contexts contexts)
        {
            Add(new OnNewPredictionCreateBackup(contexts, Services));    

            Add(new InputFeature(contexts, Services));

            Add(new VerifySelectionIdExists(contexts, Services));

            Add(new NavigationFeature(contexts, Services));

            Add(new GameEventSystems(contexts));

            Add(new HashCodeFeature(contexts, Services));   

            Add(new RemoveNewFlag(contexts));

            Add(new IncrementTick(contexts));

            Add(new VerifyNoDuplicateBackups(contexts, Services));
        }                       

        public void Initialize(byte[] allActorIds)
        {         
            Initialize();
            foreach (var actorId in allActorIds)
            {
                var actor = Contexts.actor.CreateEntity();
                actor.AddId(actorId);
                actor.AddEntityCount(0);
            }
        }

        public void AddInput(uint tickId, byte actor, List<ICommand> input)
        {
            GameLog.Add(CurrentTick, tickId, actor, input.ToArray());

            foreach (var command in input)
            {
                var inputEntity = Contexts.input.CreateEntity();
                command.Execute(inputEntity);

                inputEntity.AddTick(tickId);
                inputEntity.AddActorId(actor);
            }

            //TODO: after adding input, order input by timestamp => if commands intersect, the first one should win, timestamp should be added by server, RTT has to be considered
            //ordering by timestamp requires loopback functionality because we have to wait for server-response; at the moment commands get distributed to all clients except oneself
            //if a command comes back from server and it was our own command, the local command has to be overwritten instead of just adding it (as it is at the moment)
        }

        public void Predict()
        {
            if (!Contexts.gameState.isPredicting)
            {
                Contexts.gameState.isPredicting = true;
            }                                                        

            Services.Get<ILogService>().Trace("Predict " + CurrentTick);
            Execute();
            Cleanup();
        }

        public void Simulate()
        {
            if (Contexts.gameState.isPredicting)
            {
                Contexts.gameState.isPredicting = false;
            }

            Services.Get<ILogService>().Trace("Simulate " + CurrentTick);

            Execute();
            Cleanup();
        }     

        /// <summary>
        /// Revert all changes that were done during or after the given tick
        /// </summary>
        /// <param name="tick"></param>
        public void RevertToTick(uint tick)
        {

            Services.Get<ILogService>().Trace("Rollback to " + tick);
            //Get the actual tick that we have a snapshot for
            var resultTick = Services.Get<ISnapshotIndexService>().GetFirstIndexBefore(tick);  

            /*
             * ====================== Revert actors ======================
             * most importantly: the entity-count per actor gets reverted so the composite key (Id + ActorId) of GameEntities stays in sync
             */

            var backedUpActors = _actorContext.GetEntities(ActorMatcher.Backup).Where(e => e.backup.tick == resultTick);
            foreach (var backedUpActor in backedUpActors)
            {                          
                backedUpActor.CopyTo(
                    _actorContext.GetEntityWithId(backedUpActor.backup.actorId),                                   //Current Actor
                    true,                                                                             //Replace components
                    backedUpActor.GetComponentIndices().Except(new []{ ActorComponentsLookup.Backup }).ToArray()); //Copy everything except the backup-component
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

            //Copy old state to the entity                                      
            foreach (var backupEntity in backupEntities)
            {    
                var target = _gameContext.GetEntityWithLocalId(backupEntity.backup.localEntityId);
                backupEntity.CopyTo(target, true, backupEntity.GetComponentIndices().Except(new []{GameComponentsLookup.Backup}).ToArray());
            }

            //TODO: restore locally destroyed entities      


            Contexts.gameState.ReplaceTick(resultTick); 

            while (Contexts.gameState.tick.value < tick)
            {
                Simulate();
            }
        }
    }   
}