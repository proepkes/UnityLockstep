using Entitas;

public class EmitInputSystem : IExecuteSystem, ICleanupSystem
{
    private readonly InputContext _inputContext; 

    private readonly IParseInputService _parseInputService;

    public EmitInputSystem(Contexts contexts, IParseInputService parseInputService)
    {
        _inputContext = contexts.input;
        _parseInputService = parseInputService;
    }     

    public void Execute()
    {
        if (_inputContext.frame.SerializedInputs == null)
            return;

        foreach (var input in _inputContext.frame.SerializedInputs)
        {
            _parseInputService.Parse(_inputContext, input);
        }
    }

    public void Cleanup()
    {
        _inputContext.DestroyAllEntities();
    }
}     
