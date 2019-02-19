using System.Linq;
using Entitas;
using Lockstep.Common.Logging;
using Lockstep.Game.Interfaces;

namespace Lockstep.Game.Features.Input
{
    public class ExecuteNavigationInput : IExecuteSystem
    {
        private INavigationService _navigationService;

        private readonly GameContext _gameContext;
        readonly IGroup<InputEntity> _navigationInput;
        private readonly GameStateContext _gameStateContext;

        public ExecuteNavigationInput(Contexts contexts, ServiceContainer services)
        {                                             
            _gameContext = contexts.game;
            _gameStateContext = contexts.gameState;

            _navigationService = services.Get<INavigationService>();

            _navigationInput = contexts.input.GetGroup(InputMatcher.AllOf(
                InputMatcher.Coordinate, 
                InputMatcher.Selection,
                InputMatcher.ActorId, 
                InputMatcher.Tick));
        }    


        public void Execute()
        {
            foreach (var input in _navigationInput.GetEntities().Where(entity => entity.tick.value == _gameStateContext.tick.value))
            {
                var destination = input.coordinate.value;
                var targetActorId = input.hasTargetActorId ? input.targetActorId.value : input.actorId.value;

                var selectedEntities = _gameContext
                    .GetEntities(GameMatcher.LocalId)
                    .Where(entity =>
                        input.selection.entityIds.Contains(entity.id.value) &&
                        entity.actorId.value == targetActorId);


                Log.Trace(this, targetActorId + " moving " + string.Join(", ", selectedEntities.Select(entity => entity.id.value)));

                foreach (var entity in selectedEntities)
                {
                    _navigationService.SetDestination(entity.localId.value, destination);
                }
            }
        }
    }
}
