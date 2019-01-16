using Entitas;

public class EmitInputSystem : IExecuteSystem, ICleanupSystem
{
    private readonly InputContext _inputContext; 

    private readonly IInputParseService _inputParseService;

    public EmitInputSystem(Contexts contexts, IInputParseService inputParseService)
    {
        _inputContext = contexts.input;
        _inputParseService = inputParseService;
    }     

    public void Execute()
    {
        foreach (var input in _inputContext.frame.SerializedInputs)
        {
            _inputParseService.Parse(_inputContext, input);
        }
    }

    public void Cleanup()
    {
        _inputContext.DestroyAllEntities();
    }
}     
