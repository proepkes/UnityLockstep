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
            var changedEntities = _storage.GetChanges(tick + 1).ToList();

            foreach (var entityId in changedEntities.Where(entity => entity.isNew).Select(e => e.idReference.value))
            {
                _game.UnloadEntity(entityId);
                _gameContext.GetEntityWithId(entityId).Destroy();  
            }

            foreach (var entity in changedEntities.Where(entity => !entity.isNew))
            {
                //TODO: revert changes and add previously removed entities                              
            }           

            for (var i = tick; i <= Contexts.gameState.tick.value; i++)
            {                                                                    
                _storage.RemoveChanges(i);    
            }

            foreach (var entity in changedEntities)
            {
                entity.Destroy();
            }

            Contexts.gameState.ReplaceTick(tick);   
        }
    }
}     