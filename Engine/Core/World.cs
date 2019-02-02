using System;
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
            foreach (var command in input)
            {
                var inputEntity = Contexts.input.CreateEntity();
                command.Execute(inputEntity);

                inputEntity.AddTick(tickId);
                inputEntity.AddActorId(actor);
            }
        }

        public void Predict()
        {
            if (!Contexts.gameState.isPredicting)
            {
                Contexts.gameState.isPredicting = true;
            }                                                        

            Execute();
            Cleanup();
        }

        public void Simulate()
        {
            if (Contexts.gameState.isPredicting)
            {
                Contexts.gameState.isPredicting = false;
            }                                                                       

            Execute();
            Cleanup();
        }     

        /// <summary>
        /// Revert all changes that were done during or after the given tick
        /// </summary>
        /// <param name="tick"></param>
        public void RevertToTick(uint tick)
        {       
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
                    _actorContext.GetEntityWithId(backedUpActor.backup.actorId), //Current Actor
                    true, //Replace components
                    backedUpActor.GetComponentIndices().Except(new []{ ActorComponentsLookup.Backup }).ToArray()); //Copy everything except the backup-component
            }

            /*
            * ====================== Revert game-entities ======================      
            */

            var currentEntities = _gameContext.GetEntities(GameMatcher.LocalId);
            var backedUpEntities = _gameContext.GetEntities(GameMatcher.Backup).Where(e => e.backup.tick == resultTick).Select(entity => entity.backup.localEntityId).ToList();

            //Entities that were created in the prediction have to be destroyed              
            var invalidEntities = currentEntities.Where(entity => !backedUpEntities.Contains(entity.localId.value)); 
            foreach (var invalidEntity in invalidEntities)
            {
                //Here we have the actual entities, we can safely refer to them via the internal id
                _view.DeleteView(invalidEntity.localId.value);
                _gameContext.GetEntityWithLocalId(invalidEntity.localId.value).Destroy();
            }




            //Apply old values to the components
            //foreach (var shadow in shadows.Except(spawnedShadows))
            //{                                                                                                                
            //    var referencedEntity = currentEntities.FirstOrDefault(e => e.hasId && e.hasOwnerId && e.id.value == shadow.id.value && e.ownerId.value == shadow.ownerId.value);

            //    //Check if the entity got destroyed locally
            //    if (referencedEntity == null)
            //    {
            //        //TODO: restore locally destroyed entities
            //    }
            //    else
            //    {
            //        //Entity is in the game locally, revert to old state
            //        var currentComponents = referencedEntity.GetComponentIndices();
            //        var previousComponents = shadow.GetComponentIndices().Except(new[] { GameComponentsLookup.Shadow, GameComponentsLookup.Tick }).ToArray();

            //        var sameComponents = previousComponents.Intersect(currentComponents);
            //        var missingComponents = previousComponents.Except(currentComponents).ToArray();
            //        var onlyLocalComponents = currentComponents.Except(new[] { GameComponentsLookup.LocalId }).Except(previousComponents); 

            //        shadow.CopyTo(referencedEntity, true, sameComponents.ToArray());

            //        //CopyTo with 0 params would copy all...
            //        if (missingComponents.Length > 0)
            //        {   
            //            shadow.CopyTo(referencedEntity, false, missingComponents);
            //        }

            //        foreach (var index in onlyLocalComponents)
            //        {
            //            referencedEntity.RemoveComponent(index);
            //        }            
            //    }
            //}           

            Contexts.gameState.ReplaceTick(resultTick);
        }
    }   
}