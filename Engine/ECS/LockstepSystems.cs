using ECS.Data;
using ECS.Features;            
using Entitas;          

public sealed class LockstepSystems : Systems
{
    private readonly Contexts _contexts;

    public LockstepSystems(Contexts contexts, ServiceContainer serviceContainer)
    {
        _contexts = contexts;
        contexts.game.OnEntityCreated += (context, entity) => ((GameEntity) entity).AddId(entity.creationIndex);
        
                               
        Add(new InputFeature(contexts, serviceContainer));

        Add(new PathfindingFeature(contexts, serviceContainer));

        Add(new GameEventSystems(contexts));
    }

    public void Simulate(Frame frame)
    {
        _contexts.input.SetFrame(frame.SerializedInputs);

        Execute();
        Cleanup();                   
    }
}     