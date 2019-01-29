using System.Collections.Generic;
using BEPUutilities;
using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems.Input
{
    public class OnSpawnInputCreateEntity : ReactiveSystem<InputEntity>, IStateSystem
    {
        private uint _nextEntityId;

        private readonly IGameService _gameService;
        private readonly GameContext _gameContext;
        private readonly GameStateContext _gameStateContext;

        private readonly Dictionary<uint, List<uint>> _createdEntities = new Dictionary<uint, List<uint>>();
        private ILogService logger;

        public OnSpawnInputCreateEntity(Contexts contexts, ServiceContainer services) : base(contexts.input)
        {
            _gameService = services.Get<IGameService>();
            logger = services.Get<ILogService>();
            _gameContext = contexts.game;
            _gameStateContext = contexts.gameState;
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context)
        {
            return context.CreateCollector(InputMatcher.AllOf(InputMatcher.EntityConfigId, InputMatcher.Coordinate));
        }

        protected override bool Filter(InputEntity entity)
        {                     
            return entity.hasEntityConfigId && entity.hasCoordinate;
        }

        protected override void Execute(List<InputEntity> inputs)
        {
            foreach (var input in inputs)
            {
                var e = _gameContext.CreateEntity();

                e.AddId(_nextEntityId);
                e.AddVelocity(Vector2.Zero);
                e.AddPosition(input.coordinate.value);

                _gameService.LoadEntity(e, input.entityConfigId.value);

                if (!_createdEntities.ContainsKey(_gameStateContext.tick.value))
                {
                    _createdEntities.Add(_gameStateContext.tick.value, new List<uint>());
                }

                _createdEntities[_gameStateContext.tick.value].Add(_nextEntityId);

                _nextEntityId++;
            }
        }

        public void RevertToTick(uint tick)
        {                                                                                                                                  
            for (;tick <= _gameStateContext.tick.value; tick++)
            {                             
                if (_createdEntities.ContainsKey(tick))
                {
                    logger.Warn("Destroying " + _createdEntities[tick].Count + " Entities from tick " + tick);
                    foreach (var entityId in _createdEntities[tick])
                    {
                        _gameContext.GetEntityWithId(entityId).Destroy();
                        _nextEntityId--;
                    }

                    _createdEntities[tick].Clear();
                }
            }
        }
    }
}
