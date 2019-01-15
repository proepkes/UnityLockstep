using ECS.Data;
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
    void Process(InputContext context, Command command);
}