using Entitas;                                  

namespace Lockstep.Core.Logic.Systems.GameState
{
    public class CalculateHashCode : IInitializeSystem, IExecuteSystem
    {                                                         
        private readonly IGroup<GameEntity> _hashableEntities;

        private readonly GameStateContext _gameStateContext;

        public CalculateHashCode(Contexts contexts, ServiceContainer serviceContainer)
        {                                                                                 
            _gameStateContext = contexts.gameState;

            _hashableEntities = contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.LocalId, GameMatcher.Position).NoneOf(GameMatcher.Backup));
        }

        public void Initialize()
        {
            _gameStateContext.ReplaceHashCode(0);
        }

        public void Execute()
        {
            long hashCode = 0;
            foreach (var entity in _hashableEntities)
            {
                hashCode ^= entity.position.value.X.RawValue;
                hashCode ^= entity.position.value.Y.RawValue;
            }                   

            _gameStateContext.ReplaceHashCode(hashCode);
        }       
    }
}
