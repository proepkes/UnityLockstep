using Entitas;

namespace ECS.Systems.Navigation
{
    public class CheckDestinationReached : IExecuteSystem
    {
        //Moving entities are those who have a position and a destination
        readonly IGroup<GameEntity> _movingEntites;

        public CheckDestinationReached(Contexts contexts)
        {
            _movingEntites = contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.Movable, GameMatcher.Destination));
        }

        public void Execute()
        {
            foreach (var e in _movingEntites.GetEntities())
            {
                if ((e.position.value - e.destination.value).Length() < 1)
                {
                    e.RemoveDestination();
                }
            }
        }
    }
}
