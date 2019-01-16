using Entitas;

public class EmitInputSystem : IInitializeSystem, IExecuteSystem, ICleanupSystem
{
    private InputContext _inputContext;  

    private ICommandService _commandService;

    public EmitInputSystem(Contexts contexts, ICommandService commandService)
    {
        _inputContext = contexts.input;
        _commandService = commandService;
    }

    public void Initialize()
    {                                                               
    }

    public void Execute()
    {
        foreach (var command in _inputContext.frame.Commands)
        {
            _commandService.Process(_inputContext, command);
        }
    }

    public void Cleanup()
    {
        _inputContext.DestroyAllEntities();
    }
}     
