using ECS.Systems.Pathfinding;  

namespace ECS.Features
{
    public sealed class PathfindingFeature : Feature
    {
        public PathfindingFeature(Contexts contexts, ServiceContainer serviceContainer)
        {
            Add(new OnGameEntityMovableRegisterToPathfinder(contexts, serviceContainer.Get<IPathfindingService>()));
            Add(new CheckDestinationReached(contexts));   
            Add(new UpdatePathfinder(contexts, serviceContainer.Get<IPathfindingService>()));
            Add(new SyncAgentPosition(contexts, serviceContainer.Get<IPathfindingService>()));

        }
    }
}
