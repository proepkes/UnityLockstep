using Entitas;
using Entitas.CodeGeneration.Attributes;

[Service, Unique]
public sealed class CommandServiceComponent : IComponent
{
    public ICommandService instance;
}

[Service, Unique]
public sealed class TimeServiceComponent : IComponent
{
    public ITimeService instance;
}