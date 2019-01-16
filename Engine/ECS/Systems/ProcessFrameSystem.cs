using System.Collections.Generic;        
using Entitas;

public class ProcessFrameSystem : IInitializeSystem, IExecuteSystem
{
    private GameContext _gameContext;
    private InputContext _inputContext;
    private ServiceContext _serviceContext;

    private ICommandService _commandService;

    public ProcessFrameSystem(Contexts contexts)
    {
        _gameContext = contexts.game;
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
            _commandService.Process(_gameContext, command);
        }
    }
}     
