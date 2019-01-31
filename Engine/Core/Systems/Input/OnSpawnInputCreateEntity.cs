using System.Linq;
using BEPUutilities;
using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems.Input
{
    public class OnSpawnInputCreateEntity : IExecuteSystem
    {
        private uint _nextEntityId;

        private readonly IViewService _viewService;
        private readonly GameContext _gameContext;
        private readonly GameStateContext _gameStateContext;   
        private readonly IGroup<InputEntity> _spawnInputs;

        public OnSpawnInputCreateEntity(Contexts contexts, ServiceContainer services)
        {
            _viewService = services.Get<IViewService>();     
            _gameContext = contexts.game;
            _gameStateContext = contexts.gameState;  

            _spawnInputs = contexts.input.GetGroup(InputMatcher.AllOf(InputMatcher.EntityConfigId, InputMatcher.Coordinate,
                InputMatcher.PlayerId, InputMatcher.TickId));
        }       

        public void Execute()
        {
            foreach (var input in _spawnInputs.GetEntities().Where(entity => entity.tickId.value == _gameStateContext.tick.value))
            {                                                       
                var e = _gameContext.CreateEntity();

                e.isNew = true;
                e.AddId(_nextEntityId);
                e.AddOwnerId(input.playerId.value);

                e.AddVelocity(Vector2.Zero);
                e.AddPosition(input.coordinate.value);

                _viewService.LoadView(e, input.entityConfigId.value);    
                _nextEntityId++;  
            }                                                                                    
        }    
    }
}
