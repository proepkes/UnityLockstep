using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems
{
    public class CalculateHashCode : IInitializeSystem, IExecuteSystem
    {
        private readonly IHashService _hashService;
        private readonly IGroup<GameEntity> _hashableEntities;

        private readonly GameStateContext _gameStateContext;

        public CalculateHashCode(Contexts contexts, IHashService hashService)
        {
            _hashService = hashService;
            _gameStateContext = contexts.gameState;

            _hashableEntities = contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.LocalId, GameMatcher.Hashable));
        }

        public void Initialize()
        {
            _gameStateContext.SetHashCode(0);
        }

        public void Execute()
        {  
            _gameStateContext.ReplaceHashCode(_hashService.CalculateHashCode(_hashableEntities.GetEntities()));
        }
    }
}
