
using System.Collections.Generic;
using System.Linq;
using ECS.Data;
using ECS.Systems;
using Entitas;
using RVO;

public sealed class LockstepSystems : Systems
{
    private readonly Contexts _contexts;

    public LockstepSystems(Contexts contexts, ICollection<IService> externalServices)
    {
        _contexts = contexts;
        contexts.game.OnEntityCreated += (context, entity) => ((GameEntity) entity).AddId(entity.creationIndex);

        Add(new EmitInputSystem(contexts, externalServices.FirstOrDefault(service => service is IParseInputService) as IParseInputService));
        Add(new InputToCreateGameEntitySystem(contexts));
        Add(new LoadAssetSystem(contexts, externalServices.FirstOrDefault(service => service is IViewService) as IViewService));
        Add(new InputToGameEntityDestinationSystem(contexts)); 
        Add(new SetAgentPosition(contexts));
        Add(new GameEventSystems(contexts));
    }

    public void Simulate(Frame frame)
    {
        _contexts.input.SetFrame(frame.SerializedInputs);

        Execute();
        Cleanup();


        Simulator.Instance.doStep();
    }
}     