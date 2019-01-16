
using ECS.Systems;
using Entitas;

public sealed class LockstepSystems : Systems
{
    public LockstepSystems(Contexts contexts, ExternalServices externalServices)
    {
        contexts.game.OnEntityCreated += (context, entity) => (entity as GameEntity)?.AddId(entity.creationIndex);

        Add(new RegisterServicesSystem(contexts, externalServices));
        Add(new EmitInputSystem(contexts));
        Add(new CommandMoveSystem(contexts));
    }       
}     