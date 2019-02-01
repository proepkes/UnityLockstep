using System;
using System.Collections.Generic;
using System.Linq;
using Entitas;
using Lockstep.Core.Features;
using Lockstep.Core.Interfaces;
using Lockstep.Core.Systems;
using Lockstep.Core.Systems.GameState;

namespace Lockstep.Core
{
    public sealed class World : Feature, IWorld
    {
        private Contexts Contexts { get; }

        public Services Services { get; }

        public uint CurrentTick => Contexts.gameState.tick.value;

        public int EntitiesInCurrentTick => Contexts.game.GetEntities().Count(e => !e.isShadow);

        private readonly IViewService _view;
        private readonly GameContext _gameContext;
        private readonly INavigationService _navigation;
        private readonly IPlayerEntityIdProvider _idProvider;

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
            _idProvider = Services.Get<IPlayerEntityIdProvider>();

            _gameContext = contexts.game;

            AddFeatures(contexts);
        }

        private void AddFeatures(Contexts contexts)
        {
            Add(new OnNewPredictionCreateSnapshot(contexts, Services));    

            Add(new InputFeature(contexts, Services));

            Add(new NavigationFeature(contexts, Services));

            Add(new GameEventSystems(contexts));

            Add(new HashCodeFeature(contexts, Services));   

            Add(new RemoveNewFlag(contexts));

            Add(new IncrementTick(Contexts));

            Add(new VerifyShadows(Contexts, Services));
        }                       

        public void Initialize(byte playerId)
        {
            Initialize();
            Contexts.gameState.SetPlayerId(playerId);
        }

        public void AddInput(uint tickId, byte player, List<ICommand> input)
        {
            foreach (var command in input)
            {
                var inputEntity = Contexts.input.CreateEntity();
                command.Execute(inputEntity);

                inputEntity.AddTick(tickId);
                inputEntity.AddPlayerId(player);
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
            var currentEntities = _gameContext.GetEntities(GameMatcher
                .AllOf(
                    GameMatcher.Id,
                    GameMatcher.OwnerId)
                .NoneOf(GameMatcher.Shadow));   
            
            var resultTick = Services.Get<ISnapshotIndexService>().GetFirstIndexBefore(tick);
            var shadows = _gameContext.GetEntities(GameMatcher.Shadow).Where(e => e.tick.value == resultTick).ToList();  

            //Entities that were created in the prediction have to be destroyed              
            //var invalidEntities = new List<GameEntity>(200);
            //foreach (var spawnedShadowsPerOwner in spawnedShadows.ToLookup(e => e.ownerId.value, e => e.id.value))
            //{
            //    var ownerId = spawnedShadowsPerOwner.Key;

            //    var referencedEntities = currentEntities.Where(entity => ownerId == entity.ownerId.value && spawnedShadowsPerOwner.Contains(entity.id.value)).ToList();

            //    invalidEntities.AddRange(referencedEntities);

            //    //Reset id service for the player to last valid state, since we created IDs that may not match with other players 
            //    _idProvider.SetNext(ownerId, _idProvider.Get(ownerId) - (uint)referencedEntities.Count);
            //}

            //foreach (var invalidEntity in invalidEntities)
            //{
            //    //Here we have the actual entities, we can safely refer to them via the internal id
            //    _view.DeleteView(invalidEntity.localId.value);
            //    _gameContext.GetEntityWithLocalId(invalidEntity.localId.value).Destroy();
            //}
            
            
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
                       

            Contexts.gameState.ReplaceTick(tick);
        }
    }     
}