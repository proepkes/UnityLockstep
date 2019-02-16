using System.Linq;
using Entitas;
using Lockstep.Common.Logging;

namespace Lockstep.Core.Logic.Systems.GameState
{
    public class CalculateHashCode : IInitializeSystem, IExecuteSystem
    {                                                         
        private readonly IGroup<GameEntity> _hashableEntities;

        private readonly GameStateContext _gameStateContext;

        public CalculateHashCode(Contexts contexts)
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
            hashCode ^= _hashableEntities.count;
            foreach (var entity in _hashableEntities)
            {
                hashCode ^= entity.position.value.X.RawValue;
                hashCode ^= entity.position.value.Y.RawValue;
                if (entity.hasVelocity)
                {
                    hashCode ^= entity.velocity.value.X.RawValue;
                    hashCode ^= entity.velocity.value.Y.RawValue;
                }
                if (entity.hasDestination)
                {
                    hashCode ^= entity.destination.value.X.RawValue;
                    hashCode ^= entity.destination.value.Y.RawValue;
                }
                if (entity.hasNeighbors)
                {
                    foreach (var neighbor in entity.neighbors.neighborsECS.Where(e => e != null))
                    {   
                        hashCode ^= neighbor.actorId.value;
                        hashCode ^= neighbor.id.value;
                    }
                }
            }                   

            _gameStateContext.ReplaceHashCode(hashCode);
        }       
    }
}
