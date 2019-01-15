using Entitas;

public class ProcessFrameSystem : IExecuteSystem, ICleanupSystem
{                                           
    private readonly Contexts _contexts;
    private readonly InputContext _context;

    public ProcessFrameSystem(Contexts contexts)
    {                                                   
        _contexts = contexts;
        _context = contexts.input;    
    }         

    public void Execute()
    {   
        var entity = _context.frameEntity;       
    }

    public void Cleanup()
    {                                                  
    }
}     
