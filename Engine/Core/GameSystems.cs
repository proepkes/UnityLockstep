using System.Collections.Generic;
using System.Linq;
using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;
using Lockstep.Core.Systems;
using Lockstep.Core.Systems.GameState;    

namespace Lockstep.Core
{
    public sealed class GameSystems : Entitas.Systems, IWorld
    {
        private Contexts Contexts { get; }

        public ServiceContainer Services { get; }

        public TickId CurrentTick => Contexts.gameState.tick.value;

        public int EntitiesInCurrentTick => Contexts.game.GetEntities().Count(e => e.hasId);

        private readonly IViewService _view;        
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
                                                        
            _view = Services.Get<IViewService>();
            _navigation = Services.Get<INavigationService>();
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

        public void AddInput(TickId tickId, Dictionary<PlayerId, List<ICommand>> input)
        {
            foreach (var playerId in input.Keys)
            {
                foreach (var command in input[playerId])
                {
                    var inputEntity = Contexts.input.CreateEntity();
                    command.Execute(inputEntity);

                    inputEntity.AddPlayerId(playerId);
                    inputEntity.AddTickId(tickId);  
                }
            }
        }

        public void Tick()
        {
            Execute();
            Cleanup();
        }      

        public void RevertToTick(uint tick)
        {
            Services.Get<ILogService>().Warn("Revert to " + tick);

            //Revert all changes that were done in or after the given tick     
            var shadows = _gameContext.GetEntities().Where(e => e.hasShadow && e.shadow.tick >= tick).ToList();

            foreach (var shadow in shadows.Where(e => e.isNew).Select(e => e.shadow.entityId))
            {                         
                //_navigation.RemoveAgent(shadow);

                _view.DeleteView(shadow);
                _gameContext.GetEntityWithId(shadow).Destroy();  
            }
                                                                                                                           
            foreach (var entity in shadows.Where(e => !e.isNew))
            {
                var referencedEntity = _gameContext.GetEntityWithId(entity.shadow.entityId);
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
                    var missingComponents = previousComponents.Except(new []{ GameComponentsLookup.Shadow }).Except(currentComponents);


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