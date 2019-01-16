using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BEPUutilities;
using ECS.Data;
using Entitas;
using FixMath.NET;

public class ExternalServices
{
    public readonly ITimeService Time;
    public readonly ICommandService Command;
    public readonly IGridService Grid;
    public readonly IViewService View;

    public ExternalServices(ICollection<IService> services)
    {
        Command = services.FirstOrDefault(s => s is ICommandService) as ICommandService;
        Time = services.FirstOrDefault(s => s is ITimeService) as ITimeService;
        Grid = services.FirstOrDefault(s => s is IGridService) as IGridService;
        View = services.FirstOrDefault(s => s is IViewService) as IViewService;
    }

    public ExternalServices(ICommandService command, ITimeService time, IGridService grid, IViewService view)
    {
        Command = command;
        Time = time;
        Grid = grid;
        View = view;
    }
}

public interface IService
{

}

public interface ITimeService : IService
{
    Fix64 FixedDeltaTime { get; }
}

public interface IGridService : IService
{
    Vector2 GetWorldSize();
    Vector2 GetCellSize();
    Fix64 GetHeightScale();
}

public interface ICommandService : IService
{
    void Process(InputContext context, Command command);
}

public interface IViewService : IService
{
    // create a view from a premade asset (e.g. a prefab)
    void LoadAsset(
        Contexts contexts,
        IEntity entity,
        string assetName);
}