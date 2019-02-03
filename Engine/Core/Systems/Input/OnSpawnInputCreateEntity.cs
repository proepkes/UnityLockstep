using System.Linq;
using BEPUutilities;
using Entitas;             
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems.Input
{
    public class OnSpawnInputCreateEntity : IExecuteSystem
    {
        private readonly Services _services;
        private readonly IViewService _viewService;
        private readonly GameContext _gameContext;
        private readonly GameStateContext _gameStateContext;   
        private readonly IGroup<InputEntity> _spawnInputs;    

        private uint _localIdCounter;
        private readonly ActorContext _actorContext;

        public OnSpawnInputCreateEntity(Contexts contexts, Services services)
        {
            _services = services;
            _viewService = services.Get<IViewService>();              
            _gameContext = contexts.game;
            _gameStateContext = contexts.gameState;
            _actorContext = contexts.actor;

            _spawnInputs = contexts.input.GetGroup(
                InputMatcher.AllOf(
                    InputMatcher.EntityConfigId,
                    InputMatcher.ActorId,
                    InputMatcher.Coordinate,
                    InputMatcher.Tick));
        }       

        public void Execute()
        {                                                             
            foreach (var input in _spawnInputs.GetEntities().Where(entity => entity.tick.value == _gameStateContext.tick.value))
            {           
                var actor = _actorContext.GetEntityWithId(input.actorId.value);
                var nextEntityId = actor.entityCount.value;

                var e = _gameContext.CreateEntity();

                e.isNew = true;
                _services.Get<ILogService>().Trace(actor.id.value + " -> " + nextEntityId);

                //composite primary key
                e.AddId(nextEntityId);
                e.AddActorId(input.actorId.value);

                //unique id for internal usage
                e.AddLocalId(_localIdCounter);
                
                //some default components that every game-entity must have
                e.AddVelocity(Vector2.Zero);
                e.AddPosition(input.coordinate.value);

                _viewService.LoadView(e, input.entityConfigId.value);

                actor.ReplaceEntityCount(nextEntityId + 1);
                _localIdCounter += 1;
            }                                                                                    
        }    
    }
}
