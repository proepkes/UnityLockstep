using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems
{
    public class CalculateHashCode : IInitializeSystem, IExecuteSystem
    {
        private readonly Services _services;
        private readonly IHashService _hashService;
        private readonly IGroup<GameEntity> _hashableEntities;

        private readonly GameStateContext _gameStateContext;

        public CalculateHashCode(Contexts contexts, Services services)
        {
            _services = services;
            _hashService = services.Get<IHashService>();

            _gameStateContext = contexts.gameState;

            _hashableEntities = contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.LocalId, GameMatcher.Position).NoneOf(GameMatcher.Backup));
        }

        public void Initialize()
        {
            _gameStateContext.ReplaceHashCode(0);
        }

        public void Execute()
        {  
            _gameStateContext.ReplaceHashCode(_hashService.CalculateHashCode(_hashableEntities.GetEntities(), _gameStateContext, _services.Get<ILogService>()));
        }
    }
}
