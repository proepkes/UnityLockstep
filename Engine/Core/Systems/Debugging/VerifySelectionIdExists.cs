using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems.Debugging
{
    public class VerifySelectionIdExists : IExecuteSystem
    {
        private readonly Services _services;                        
        private readonly GameContext _gameContext;
        private readonly InputContext _inputContext;
        private readonly GameStateContext _gameStateContext;

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
                var ents = _gameContext.GetEntities(GameMatcher.LocalId)
                    .Where(entity => entity.actorId.value == input.actorId.value).Select(entity => entity.id.value);
                var missing = input.selection.entityIds.Where(u => !ents.Contains(u)).ToList();
                if (missing.Any())
                {   
                    _services.Get<ILogService>().Warn(missing.Count + " missing for actor: " + input.actorId.value);
                }
                
            }
        }
    }
}
