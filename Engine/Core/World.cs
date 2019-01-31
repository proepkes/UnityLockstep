using System.Collections.Generic;
using System.Linq;
using Entitas;
using Lockstep.Core.Interfaces;
using Lockstep.Core.Systems;
using Lockstep.Core.Systems.GameState;    

namespace Lockstep.Core
{
    public sealed class World : Entitas.Systems, IWorld
    {
        private Contexts Contexts { get; }

        public ServiceContainer Services { get; }

        public uint CurrentTick => Contexts.gameState.tick.value;

        public int EntitiesInCurrentTick => Contexts.game.GetEntities().Count(e => !e.isShadow);

        private readonly IViewService _view;        
        private readonly GameContext _gameContext;
        private readonly INavigationService _navigation;
        private readonly IPlayerEntityIdProvider _idProvider;

        public World(Contexts contexts, params IService[] additionalServices)
        {
            Contexts = contexts;      

            Services = new ServiceContainer();
            foreach (var service in additionalServices)
            {
                Services.Register(service);
            }
                                                        
            _view = Services.Get<IViewService>();
            _navigation = Services.Get<INavigationService>();
            _idProvider = Services.Get<IPlayerEntityIdProvider>();
            _gameContext = contexts.game;  

            Add(new CoreSystems(contexts, Services));
                                                            
            Add(new StoreNewOrChangedEntities(contexts)); 

            Add(new RemoveNewFlag(contexts));

            Add(new IncrementTick(Contexts));
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
              
        public void Tick()
        {
            Execute();
            Cleanup();
        }

        /// <summary>
        /// Revert all changes that were done during or after the given tick
        /// </summary>
        /// <param name="tick"></param>
        public void RevertToTick(uint tick)
        {
            Services.Get<ILogService>().Warn("Revert to " + tick);
             
            var shadows = _gameContext.GetEntities().Where(e => e.isShadow && e.tick.value >= tick).ToList();

            var shadowsOfNewEntities = shadows.Where(e => e.isNew);

            //A shadow refers to its entity through ownerId + id
            var shadowsOfNewEntitiesPerPlayer = shadows.Where(e => e.isNew).ToLookup(e => e.ownerId.value, e => e.id.value);

            var invalidEntities = new List<GameEntity>(200);
            foreach (var shadowsPerOwner in shadowsOfNewEntitiesPerPlayer)
            {
                var ownerId = shadowsPerOwner.Key;

                var invalidShadowsOfOwner = _gameContext.GetEntities(
                        GameMatcher.AllOf(
                            GameMatcher.Id,
                            GameMatcher.OwnerId))
                    .Where(entity => ownerId == entity.ownerId.value && shadowsPerOwner.Contains(entity.id.value)).ToList();

                invalidEntities.AddRange(invalidShadowsOfOwner); 

                //Reset id service for the player to last valid state, since we created IDs that may not match with other players 
                _idProvider.SetNext(ownerId, _idProvider.Get(ownerId) - (uint)invalidShadowsOfOwner.Count);
            }

            foreach (var invalidEntity in invalidEntities)
            {
                //Here we have the actual entities, we can safely refer to them via the internal id
                _view.DeleteView(invalidEntity.localId.value);
                _gameContext.GetEntityWithLocalId(invalidEntity.localId.value).Destroy();
            }

            //shadowsOfChangedEntities could contain shadowsOfNewEntities when a created entity changes in later ticks => 'e.isNew' is false one tick after an entity.
            //That Entity has already been destroyed above so these changes don't have to be considered
            //TODO: keep isNew = true as long as the entity doesn't change
            var shadowsOfChangedEntities = shadows.Where(e => !e.isNew).Except(shadowsOfNewEntities);
            foreach (var entity in shadows.Where(e => !e.isNew))
            {
                //var referencedEntity = _gameContext.GetEntityWithId(entity.shadow.entityId);
                ////Check if the entity got destroyed locally
                //if (referencedEntity == null)
                //{     
                //    //TODO: restore locally destroyed entities
                //}
                //else
                //{
                //    //Entity is in the game locally, revert to old state
                //    var currentComponents = referencedEntity.GetComponentIndices();
                //    var previousComponents = entity.GetComponentIndices();

                //    var sameComponents = previousComponents.Intersect(currentComponents);
                //    var onlyLocalComponents = currentComponents.Except(new[] {GameComponentsLookup.Id }).Except(previousComponents);
                //    var missingComponents = previousComponents.Except(new []{ GameComponentsLookup.Shadow }).Except(currentComponents);


                //    Services.Get<ILogService>().Warn("sameComponents: " + sameComponents.Count());
                //    Services.Get<ILogService>().Warn("onlyLocalComponents: " + onlyLocalComponents.Count());
                //    Services.Get<ILogService>().Warn("missingComponents: " + missingComponents.Count());
                //    foreach (var index in sameComponents)
                //    {                                                         
                //        referencedEntity.ReplaceComponent(index, entity.GetComponent(index));  
                //    }

                //    foreach (var index in onlyLocalComponents)
                //    {                                               
                //        referencedEntity.RemoveComponent(index);
                //    }

                //    foreach (var index in missingComponents)
                //    {                                                   
                //        referencedEntity.AddComponent(index, entity.GetComponent(index));
                //    }
                //}
            }


            //TODO: cleanup shadow-entities < last validated tick
            //for (var i = tick; i <= Contexts.gameState.tick.value; i++)
            //{                                                                    
            //    _storage.RemoveChanges(i);    
            //}

            //Reverted to a tick in the past => all predictions are invalid now, delete them
            foreach (var entity in shadows)
            {
                entity.Destroy();
            }              

            Contexts.gameState.ReplaceTick(tick);   
        }
    }
}     