using ECS.Systems.Pathfinding;

namespace ECS.Features
{
    public sealed class PathfindingFeature : Feature
    {
        public PathfindingFeature(Contexts contexts, IPathfindingService pathfindingService)
        {
            Add(new OnGameEntityMovableRegisterToPathfinder(contexts, pathfindingService));
            Add(new CheckDestinationReached(contexts));   
            Add(new UpdatePathfinder(contexts, pathfindingService));
            Add(new SyncAgentPosition(contexts, pathfindingService));

        }
    }
}
