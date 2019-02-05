using Entitas;
using Lockstep.Core.Services;
using Lockstep.Game.Services;

namespace Lockstep.Game.Systems
{
    public class CalculateHashCode : IInitializeSystem, IExecuteSystem
    {
        private readonly ServiceContainer serviceContainer;
        private readonly IHashService _hashService;
        private readonly IGroup<GameEntity> _hashableEntities;

        private readonly GameStateContext _gameStateContext;

        public CalculateHashCode(Contexts contexts, ServiceContainer serviceContainer)
        {
            this.serviceContainer = serviceContainer;
            _hashService = serviceContainer.Get<IHashService>();

            _gameStateContext = contexts.gameState;

            _hashableEntities = contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.LocalId, GameMatcher.Position).NoneOf(GameMatcher.Backup));
        }

        public void Initialize()
        {
            _gameStateContext.ReplaceHashCode(0);
        }

        public void Execute()
        {  
            _gameStateContext.ReplaceHashCode(_hashService.CalculateHashCode(_hashableEntities.GetEntities()));
        }
    }
}
