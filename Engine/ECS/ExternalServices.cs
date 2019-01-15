using ECS.Data;
using Entitas;
using FixMath.NET;

public class ExternalServices
{
    public readonly ITimeService Time;
    public readonly ICommandService Command;   

    public ExternalServices(ICommandService command, ITimeService time)
    {
        Command = command;
        Time = time;
    }
}

public interface ITimeService
{
    Fix64 FixedDeltaTime { get; }
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