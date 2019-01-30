using System.Linq;
using Lockstep.Core.Interfaces;
using Lockstep.Core.Systems;
using Lockstep.Core.Systems.GameState;    

namespace Lockstep.Core
{
    public sealed class GameSystems : Entitas.Systems, ITickable
    {
        private Contexts Contexts { get; }  

        public ServiceContainer Services { get; }

        public uint CurrentTick => Contexts.gameState.tick.value;

        public int EntitiesInCurrentTick => Contexts.game.GetEntities().Count(e => e.hasId);

        private readonly IGameService _game;
        private readonly IStorageService _storage;
        private readonly GameContext _gameContext;

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
            var entityReferences = _storage.GetChanges(tick + 1).Select(id => _gameContext.GetEntityWithIdReference(id)).ToList(); 

            foreach (var entityId in entityReferences.Where(e => e.isNew).Select(e => e.idReference.referenceId))
            {
                _game.UnloadEntity(entityId);
                _gameContext.GetEntityWithId(entityId).Destroy();  
            }

            //TODO: select will fail if the entity got destroyed in the local simulation but the changebuffer has changes
            foreach (var entity in entityReferences.Where(e => !e.isNew).Select(e => _gameContext.GetEntityWithId(e.idReference.referenceId)))
            {
                //TODO: revert changes and add previously removed entities                              
            }           

            for (var i = tick; i <= Contexts.gameState.tick.value; i++)
            {                                                                    
                _storage.RemoveChanges(i);    
            }

            foreach (var entity in entityReferences)
            {
                entity.Destroy();
            }

            Contexts.gameState.ReplaceTick(tick);   
        }
    }
}     