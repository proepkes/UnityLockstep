using System.Collections.Generic;
using System.Linq;
using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems.Debugging
{
    public class VerifySelectionIdExists : IExecuteSystem
    {
        private readonly Services _services;                        
        private readonly GameContext _gameContext;
        private InputContext _inputContext;
        private GameStateContext _gameStateContext;

        public VerifySelectionIdExists(Contexts contexts, Services services) 
        {
            _services = services;                                    
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
                foreach (var id in input.selection.entityIds)
                {
                    if (!_gameContext.GetEntities(GameMatcher.LocalId).Where(entity => entity.actorId.value == input.actorId.value).Select(entity => entity.id.value).Contains(id))
                    {
                        _services.Get<ILogService>().Warn("Id mismatch for actor: " + input.actorId.value);
                    }
                }
            }
        }
    }
}
