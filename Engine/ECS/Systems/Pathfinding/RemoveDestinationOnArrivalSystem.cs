using Entitas;
using RVO;

namespace ECS.Systems.Pathfinding
{
    public class RemoveDestinationOnArrivalSystem : IExecuteSystem
    {
        //Moving entities are those who have a position and a destination
        readonly IGroup<GameEntity> _movingEntites;

        public RemoveDestinationOnArrivalSystem(Contexts contexts)
        {
            _movingEntites = contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.Position, GameMatcher.Destination));
        }

        public void Execute()
        {
            foreach (GameEntity e in _movingEntites.GetEntities())
            {
                if (RVOMath.absSq(e.position.value - e.destination.value) < 400)
                {
                    e.RemoveDestination();
                }
            }
        }
    }
}
