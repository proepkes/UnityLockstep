using System.Linq;
using BEPUutilities;
using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems.Input
{
    public class OnSpawnInputCreateEntity : IExecuteSystem
    {                                  
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
            var newId = _gameContext.GetEntities(GameMatcher.Id).Max(e => e.id.value) + 1;

            //TODO: order by timestamp instead of playerId => if commands intersect, the first one should win, timestamp should be added by server, RTT has to be considered                                                                 
            foreach (var input in _spawnInputs.GetEntities().Where(entity => entity.tickId.value == _gameStateContext.tick.value).OrderBy(entity => entity.playerId.value))
            {                                                       
                var e = _gameContext.CreateEntity();

                e.isNew = true;
                e.AddId(newId++);
                e.AddOwnerId(input.playerId.value);

                e.AddVelocity(Vector2.Zero);
                e.AddPosition(input.coordinate.value);

                _viewService.LoadView(e, input.entityConfigId.value);   
            }                                                                                    
        }    
    }
}
