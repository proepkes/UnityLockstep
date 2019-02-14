using System.Linq;
using Entitas;
using Lockstep.Common.Logging;

namespace Lockstep.Game.Features.Input
{
    public class VerifySelectionIdExists : IExecuteSystem
    {                       
        private readonly GameContext _gameContext;
        private readonly InputContext _inputContext;
        private readonly GameStateContext _gameStateContext;

        public VerifySelectionIdExists(Contexts contexts) 
        {                                
            _gameContext = contexts.game;
            _inputContext = contexts.input;
            _gameStateContext = contexts.gameState;
        }                 

        public void Execute()
        {
            foreach (var input in _inputContext.GetEntities(
                    InputMatcher.AllOf(
                        InputMatcher.Tick, 
                        InputMatcher.Coordinate, 
                        InputMatcher.Selection, 
                        InputMatcher.ActorId))
                .Where(entity => entity.tick.value < _gameStateContext.tick.value))
            {

                var targetActorId = input.hasTargetActorId ? input.targetActorId.value : input.actorId.value;

                var ents = _gameContext.GetEntities(GameMatcher.LocalId)
                    .Where(entity => entity.actorId.value == targetActorId).Select(entity => entity.id.value);
                var missing = input.selection.entityIds.Where(u => !ents.Contains(u)).ToList();
                if (missing.Any())
                {   
                    Log.Warn(this, missing.Count + " missing for actor: " + targetActorId + " (command from " + input.actorId.value + ")");
                }
                
            }
        }
    }
}
