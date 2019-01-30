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

            Add(new StoreNewEntities(contexts, Services));
            Add(new StoreChangedEntities(contexts, Services));
        }

        public void Tick(ICommand[] input)
        {
            Contexts.input.SetCommands(input);
                        
            Execute();
            Cleanup();
        }

        public void RevertToTick(uint tick)
        {
            //Remove all entities that were created after the given tick
            var newEntities = _storage.GetAllNew(tick + 1);   
            foreach (var entityId in newEntities)
            {
                _game.UnloadEntity(entityId);
                _gameContext.GetEntityWithId(entityId).Destroy();  
            }

            //TODO: revert changes and add previously removed entities

            for (var i = tick; i <= Contexts.gameState.tick.value; i++)
            {
                //TODO: storage.remove = memory leak? entity.destroy required?
                _storage.RemoveChanges(i);
                _storage.RemoveNewEntites(i);
            }                                                                                                                         

            Contexts.gameState.ReplaceTick(tick);   
        }
    }
}     