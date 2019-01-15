using Entitas;

public sealed class TickSystem : IInitializeSystem, IExecuteSystem
{

    private readonly Contexts _contexts;
    private readonly InputContext _context;

    public TickSystem(Contexts contexts)
    {
        _contexts = contexts;
        _context = contexts.input;
    }

    public void Initialize()
    {
        _context.SetTick(0);
    }

    public void Execute()
    {
        if (_contexts.meta.isGameStatePaused)
        {
            return;
        }

        InputEntity entity = _context.tickEntity;
        entity.ReplaceTick(entity.tick.value + 1);
    }
}