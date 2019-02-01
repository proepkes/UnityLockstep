using System.Collections.Generic;
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
        private readonly IPlayerEntityIdProvider _idService;

        private uint _localIdCounter;    

        public OnSpawnInputCreateEntity(Contexts contexts, Services services)
        {
            _viewService = services.Get<IViewService>();
            _idService = services.Get<IPlayerEntityIdProvider>();
            _gameContext = contexts.game;
            _gameStateContext = contexts.gameState;  

            _spawnInputs = contexts.input.GetGroup(
                InputMatcher.AllOf(
                    InputMatcher.EntityConfigId,
                    InputMatcher.PlayerId,
                    InputMatcher.Coordinate,
                    InputMatcher.Tick));
        }       

        public void Execute()
        {      
            //TODO: order by timestamp instead of playerId => if commands intersect, the first one should win, timestamp should be added by server, RTT has to be considered                                                                 
            foreach (var input in _spawnInputs.GetEntities().Where(entity => entity.tick.value == _gameStateContext.tick.value).OrderBy(entity => entity.playerId.value))
            {          
                var e = _gameContext.CreateEntity();

                e.isNew = true;

                //composite primary key
                e.AddId(_idService.GetNext(input.playerId.value));
                e.AddOwnerId(input.playerId.value);

                e.AddLocalId(_localIdCounter);  
                e.AddVelocity(Vector2.Zero);
                e.AddPosition(input.coordinate.value);

                _viewService.LoadView(e, input.entityConfigId.value);

                _localIdCounter += 1;
            }                                                                                    
        }    
    }
}
