namespace ECS.Systems.Pathfinding
{
    public sealed class PathfindingFeature : Feature
    {
        public PathfindingFeature(Contexts contexts, IPathfindingService pathfindingService)
        {
            Add(new RegisterMovableEntityToPathfinderSystem(contexts, pathfindingService));
            Add(new RemoveDestinationOnArrivalSystem(contexts));   
            Add(new UpdatePathfinderSystem(contexts, pathfindingService));
            Add(new SyncAgentPositionSystem(contexts, pathfindingService));

        }
    }
}
