
using System.Collections.Generic;
using System.Linq;
using ECS.Data;
using ECS.Features;     
using ECS.Systems.Pathfinding;
using Entitas;                      

public sealed class LockstepSystems : Systems
{
    private readonly Contexts _contexts;

    public LockstepSystems(Contexts contexts, ICollection<IService> externalServices)
    {
        _contexts = contexts;
        contexts.game.OnEntityCreated += (context, entity) => ((GameEntity) entity).AddId(entity.creationIndex);
        
                               
        Add(new InputFeature(contexts, externalServices));

        Add(new PathfindingFeature(contexts, externalServices.FirstOrDefault(service => service is IPathfindingService) as IPathfindingService));

        Add(new GameEventSystems(contexts));
    }

    public void Simulate(Frame frame)
    {
        _contexts.input.SetFrame(frame.SerializedInputs);

        Execute();
        Cleanup();                   
    }
}     