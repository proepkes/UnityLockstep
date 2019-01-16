using System.Collections.Generic;        
using Entitas;

public class EmitInputSystem : IInitializeSystem, IExecuteSystem, ICleanupSystem
{
    private InputContext _inputContext;
    private ServiceContext _serviceContext;

    readonly IGroup<InputEntity> _inputs;

    private ICommandService _commandService;

    public EmitInputSystem(Contexts contexts)
    {
        _inputContext = contexts.input;
        _serviceContext = contexts.service;
    }

    public void Initialize()
    {
        _commandService = _serviceContext.commandService.instance;
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
