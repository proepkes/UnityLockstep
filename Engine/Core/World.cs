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
            var currentEntities = _gameContext.GetEntities(GameMatcher
                .AllOf(
                    GameMatcher.Id,
                    GameMatcher.OwnerId)
                .NoneOf(GameMatcher.Shadow));

            //Get first occurence of each shadow per entity
            var shadows = _gameContext.GetEntities(GameMatcher.Shadow).Where(e => e.tick.value >= tick).OrderBy(e => e.tick.value).Distinct(new ShadowEqualityComparer()).ToList();

            var spawnedShadows = shadows.Where(e => e.isNew).ToList();

            //A shadow refers to its entity through ownerId + id, create a lookup per player to adjust the IDs later on              
            var invalidEntities = new List<GameEntity>(200);
            foreach (var spawnedShadowsPerOwner in spawnedShadows.ToLookup(e => e.ownerId.value, e => e.id.value))
            {
                var ownerId = spawnedShadowsPerOwner.Key;

                var referencedEntities = currentEntities.Where(entity => ownerId == entity.ownerId.value && spawnedShadowsPerOwner.Contains(entity.id.value)).ToList();

                invalidEntities.AddRange(referencedEntities);

                //Reset id service for the player to last valid state, since we created IDs that may not match with other players 
                _idProvider.SetNext(ownerId, _idProvider.Get(ownerId) - (uint)referencedEntities.Count);
            }

            foreach (var invalidEntity in invalidEntities)
            {
                //Here we have the actual entities, we can safely refer to them via the internal id
                _view.DeleteView(invalidEntity.localId.value);
                _gameContext.GetEntityWithLocalId(invalidEntity.localId.value).Destroy();
            }
                                                       
            foreach (var shadow in shadows.Except(spawnedShadows))
            {                                                                                                                
                var referencedEntity = currentEntities.FirstOrDefault(e => e.hasId && e.hasOwnerId && e.id.value == shadow.id.value && e.ownerId.value == shadow.ownerId.value);

                //Check if the entity got destroyed locally
                if (referencedEntity == null)
                {
                    //TODO: restore locally destroyed entities
                }
                else
                {
                    //Entity is in the game locally, revert to old state
                    var currentComponents = referencedEntity.GetComponentIndices();
                    var previousComponents = shadow.GetComponentIndices().Except(new[] { GameComponentsLookup.Shadow, GameComponentsLookup.Tick }).ToArray();

                    var sameComponents = previousComponents.Intersect(currentComponents);
                    var missingComponents = previousComponents.Except(currentComponents).ToArray();
                    var onlyLocalComponents = currentComponents.Except(new[] { GameComponentsLookup.LocalId }).Except(previousComponents); 

                    shadow.CopyTo(referencedEntity, true, sameComponents.ToArray());

                    //CopyTo with 0 params would copy all...
                    if (missingComponents.Length > 0)
                    {   
                        shadow.CopyTo(referencedEntity, false, missingComponents);
                    }

                    foreach (var index in onlyLocalComponents)
                    {
                        referencedEntity.RemoveComponent(index);
                    }

                }
            }      

            //Reverted to a tick in the past => all shadows are invalid now, delete them
            foreach (var entity in shadows)
            {
                entity.Destroy();
            }


            currentEntities = _gameContext.GetEntities(GameMatcher
                .AllOf(
                    GameMatcher.Id,
                    GameMatcher.OwnerId)
                .NoneOf(GameMatcher.Shadow));
            var x = _gameContext.GetEntities(GameMatcher
                .AllOf(GameMatcher.Shadow));

            Contexts.gameState.ReplaceTick(tick);
        }
    }
    public class ShadowEqualityComparer : IEqualityComparer<GameEntity>
    {
        public bool Equals(GameEntity x, GameEntity y)
        {
            return x.id.value == y.id.value && x.ownerId.value == y.ownerId.value;
        }

        public int GetHashCode(GameEntity obj)
        {
            return (int) (obj.id.value << 8 + obj.ownerId.value);
        }
    }
}