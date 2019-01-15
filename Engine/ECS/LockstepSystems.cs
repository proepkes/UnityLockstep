
using Entitas;

public sealed class LockstepSystems : Systems
{
    public LockstepSystems(Contexts contexts, ExternalServices externalServices)
    {        
        contexts.game.OnEntityCreated += AddId;

        Add(new RegisterServicesSystem(contexts, externalServices));
        Add(new ProcessFrameSystem(contexts));
    }

    public void AddId(IContext context, IEntity entity)
    {
        (entity as GameEntity).AddId(entity.creationIndex);
    }
}     