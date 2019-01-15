using System.Collections.Generic;
using System.Linq;
using Entitas;

public class ProcessFrameSystem : ReactiveSystem<InputEntity>, IInitializeSystem
{
    private readonly Contexts _contexts;
    private ICommandService _commandService;

    public ProcessFrameSystem(Contexts contexts) : base(contexts.input)
    {
        _contexts = contexts;
    }

    public void Initialize()
    {
        _commandService = _contexts.service.commandService.instance;
    }

    protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context)
    {          
        return context.CreateCollector(InputMatcher.Frame);
    }

    protected override bool Filter(InputEntity entity)
    {                                   
        return entity.hasFrame && entity.frame.Commands != null;
    }

    protected override void Execute(List<InputEntity> entities)
    {           
        var entity = entities.SingleEntity();
        foreach (var command in entity.frame.Commands)
        {                                              
            _commandService.Process(_contexts.game, command);
        }
    }
}     
