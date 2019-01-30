﻿using System.Collections.Generic;
using System.Linq;          
using Lockstep.Core.Interfaces;
using Lockstep.Core.Systems;
using Lockstep.Core.Systems.GameState;    

namespace Lockstep.Core
{
    public sealed class GameSystems : Entitas.Systems, IWorld
    {
        private Contexts Contexts { get; }

        private ServiceContainer Services { get; }

        public uint CurrentTick => Contexts.gameState.tick.value;

        public int EntitiesInCurrentTick => Contexts.game.GetEntities().Count(e => e.hasId);

        private readonly IGameService _game;        
        private readonly GameContext _gameContext;
        private readonly INavigationService _navigation;

        public GameSystems(Contexts contexts, params IService[] additionalServices)
        {
            Contexts = contexts;      

            Services = new ServiceContainer();
            foreach (var service in additionalServices)
            {
                Services.Register(service);
            }
                                                        
            _game = Services.Get<IGameService>();
            _navigation = Services.Get<INavigationService>();
            _gameContext = contexts.game;

            Add(new IncrementTick(Contexts));

            Add(new CoreSystems(contexts, Services));
                                                            
            Add(new StoreNewOrChangedEntities(contexts, Services)); 

            Add(new RemoveNewFlag(contexts));
        }

        public void Initialize(byte playerId)
        {
            Initialize();
            Contexts.gameState.SetPlayerId(playerId);
        }

        public void Tick(Dictionary<byte, List<ICommand>> input)
        {
            Contexts.input.SetCommands(input);
                        
            Execute();
            Cleanup();
        }

        public void RevertToTick(uint tick)
        {
            Services.Get<ILogService>().Warn("Revert to " + tick);

            //Revert all changes that were done after the given tick     
            var entityReferences = _gameContext.GetEntities().Where(e => e.hasIdReference && e.idReference.tick > tick).Select(id => _gameContext.GetEntityWithIdReference(id.idReference.referenceId)).ToList();

            foreach (var entityId in entityReferences.Where(e => e.isNew).Select(e => e.idReference.referenceId))
            {
                _navigation.RemoveAgent(entityId);

                _game.UnloadEntity(entityId);
                _gameContext.GetEntityWithId(entityId).Destroy();  
            }
                                                                                                                           
            foreach (var entity in entityReferences.Where(e => !e.isNew))
            {
                var referencedEntity = _gameContext.GetEntityWithId(entity.idReference.referenceId);
                //Check if the entity got destroyed locally
                if (referencedEntity == null)
                {     
                    //TODO: restore locally destroyed entities
                }
                else
                {
                    //Entity is in the game locally, revert to old state
                    var currentComponents = referencedEntity.GetComponentIndices();
                    var previousComponents = entity.GetComponentIndices();

                    var sameComponents = previousComponents.Intersect(currentComponents);
                    var onlyLocalComponents = currentComponents.Except(new[] {GameComponentsLookup.Id }).Except(previousComponents);
                    var missingComponents = previousComponents.Except(new []{ GameComponentsLookup.IdReference }).Except(currentComponents);


                    Services.Get<ILogService>().Warn("sameComponents: " + sameComponents.Count());
                    Services.Get<ILogService>().Warn("onlyLocalComponents: " + onlyLocalComponents.Count());
                    Services.Get<ILogService>().Warn("missingComponents: " + missingComponents.Count());
                    foreach (var index in sameComponents)
                    {                                                         
                        referencedEntity.ReplaceComponent(index, entity.GetComponent(index));  
                    }

                    foreach (var index in onlyLocalComponents)
                    {                                               
                        referencedEntity.RemoveComponent(index);
                    }

                    foreach (var index in missingComponents)
                    {                                                   
                        referencedEntity.AddComponent(index, entity.GetComponent(index));
                    }
                }
            }


            //TODO: cleanup backup-entities < last validated tick
            //for (var i = tick; i <= Contexts.gameState.tick.value; i++)
            //{                                                                    
            //    _storage.RemoveChanges(i);    
            //}

            //Reverted to a tick in the past => all predictions are invalid now, delete them
            foreach (var entity in entityReferences)
            {
                entity.Destroy();
            }              

            Contexts.gameState.ReplaceTick(tick);   
        }
    }
}     