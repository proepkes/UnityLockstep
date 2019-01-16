using BEPUutilities;
using ECS.Data;
using Entitas;
using FixMath.NET;

public class ExternalServices
{
    public readonly ITimeService Time;
    public readonly ICommandService Command;
    public readonly IGridService Grid;

    public ExternalServices(ICommandService command, ITimeService time, IGridService grid)
    {
        Command = command;
        Time = time;
        Grid = grid;
    }
}

public interface ITimeService
{
    Fix64 FixedDeltaTime { get; }
}

public interface IGridService
{
    Vector2 GetWorldSize();
    Vector2 GetCellSize();
    Fix64 GetHeightScale();
}

public interface ICommandService
{
    void Process(GameContext context, Command command);
}

public interface IViewService
{
    // create a view from a premade asset (e.g. a prefab)
    void LoadAsset(
        Contexts contexts,
        IEntity entity,
        string assetName);
}