using ECS.Services;

public class Services
{
    public readonly ICommandService Command;   

    public Services(ICommandService command)
    {
        Command = command;    
    }
}