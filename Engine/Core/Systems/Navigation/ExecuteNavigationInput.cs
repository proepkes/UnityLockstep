using System.Linq;
using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems.Navigation
{
    public class ExecuteNavigationInput : IExecuteSystem
    {
        private readonly Services _services;
        private readonly INavigationService _navigationService;
        private readonly GameContext _gameContext;
        readonly IGroup<InputEntity> _navigationInput;
        private readonly GameStateContext _gameStateContext;

        public ExecuteNavigationInput(Contexts contexts, Services services)
        {
            _services = services;
            _navigationService = services.Get<INavigationService>();
            _gameContext = contexts.game;
            _gameStateContext = contexts.gameState;

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


                _services.Get<ILogService>().Trace(targetActorId + " moving " + string.Join(", ", selectedEntities.Select(entity => entity.id.value)));

                foreach (var entity in selectedEntities)
                {
                    entity.ReplaceDestination(destination);

                    //_navigationService.SetAgentDestination(entityId, destination);            
                }
            }
        }
    }
}
