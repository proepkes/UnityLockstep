
using System.Collections.Generic;
using System.Linq;
using ECS.Systems;
using Entitas;

public sealed class LockstepSystems : Systems
{
    public LockstepSystems(Contexts contexts, ICollection<IService> externalServices)
    {
        contexts.game.OnEntityCreated += (context, entity) => ((GameEntity) entity).AddId(entity.creationIndex);

        Add(new LoadAssetSystem(contexts, externalServices.FirstOrDefault(service => service is IViewService) as IViewService));
        Add(new EmitInputSystem(contexts, externalServices.FirstOrDefault(service => service is IInputParseService) as IInputParseService));
        Add(new InputToGameEntityDestinationSystem(contexts));  
    }       
}     