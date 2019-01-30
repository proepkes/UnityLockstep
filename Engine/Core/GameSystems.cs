using System.Linq;
using DesperateDevs.Utils;
using Lockstep.Core.Interfaces;
using Lockstep.Core.Systems;
using Lockstep.Core.Systems.GameState;    

namespace Lockstep.Core
{
    public sealed class GameSystems : Entitas.Systems, ITickable
    {
        private Contexts Contexts { get; }

        private ServiceContainer Services { get; }

        public uint CurrentTick => Contexts.gameState.tick.value;

        public int EntitiesInCurrentTick => Contexts.game.GetEntities().Count(e => e.hasId);

        private readonly IGameService _game;
        private readonly IStorageService _storage;
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

            _storage = Services.Get<IStorageService>();
            _game = Services.Get<IGameService>();
            _navigation = Services.Get<INavigationService>();
            _gameContext = contexts.game;

            Add(new IncrementTick(Contexts));

            Add(new CoreSystems(contexts, Services));
                                                            
            Add(new StoreNewOrChangedEntities(contexts, Services)); 

            Add(new RemoveNewFlag(contexts));
        }

        public void Tick(ICommand[] input)
        {
            Contexts.input.SetCommands(input);
                        
            Execute();
            Cleanup();
        }

        public void RevertToTick(uint tick)
        {
            //Revert all changes that were done after the given tick   
            var entityReferences = _storage.GetFirstChangeOccurences(tick + 1).Select(id => _gameContext.GetEntityWithIdReference(id)).ToList(); 

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
                    var onlyLocalComponents = currentComponents.Except(new[] {GameComponentsLookup.Id }).Except(previousComponents).ToList();
                    var missingComponents = previousComponents.Except(new []{ GameComponentsLookup.IdReference }).Except(currentComponents).ToList();
                                                          
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

            for (var i = tick; i <= Contexts.gameState.tick.value; i++)
            {                                                                    
                _storage.RemoveChanges(i);    
            }

            //Reverted to a tick in the past => all our nice predictions are invalid now, delete them
            foreach (var entity in entityReferences)
            {
                entity.Destroy();
            }              

            Contexts.gameState.ReplaceTick(tick);   
        }
    }
}     