using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entitas;

namespace ECS.Systems
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

            _hashableEntities = contexts.game.GetGroup(GameMatcher.HashCode);
        }

        public void Initialize()
        {
            _gameStateContext.SetGameHashCode(0);
        }

        public void Execute()
        {
            long gameHashCode = 0;
            Parallel.ForEach(_hashableEntities.GetEntities(), entity =>
            {
                var hashCode = _hashService.GetHashCode(entity);
                entity.ReplaceHashCode(hashCode);

                gameHashCode ^= hashCode;
            });
            _gameStateContext.ReplaceGameHashCode(gameHashCode);
        }
    }
}
