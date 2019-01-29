using System.Collections.Generic;
using System.Linq;
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
        private readonly ILogService _logger;

        public OnSpawnInputCreateEntity(Contexts contexts, ServiceContainer services) : base(contexts.input)
        {
            _gameService = services.Get<IGameService>();
            _logger = services.Get<ILogService>();
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

            _logger?.Warn(_gameStateContext.tick.value + ": " + inputs.Count + " added");
        }

        public void RevertToTick(uint tick)
        {
            var t = tick;
            var count = 0;
            for (;tick <= _gameStateContext.tick.value; tick++)
            {                             
                if (_createdEntities.ContainsKey(tick))
                {
                    foreach (var entityId in _createdEntities[tick])
                    {
                        _gameContext.GetEntities().First(entity => entity.id.value == entityId).Destroy();
                        _nextEntityId--;
                        count++;
                    }

                    _createdEntities[tick].Clear();
                }
            }

            _logger?.Warn(t + " -> " + tick + ": " + count + " destroyed");
        }
    }
}
