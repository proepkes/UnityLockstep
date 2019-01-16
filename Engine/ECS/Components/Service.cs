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

[Service, Unique]
public sealed class ViewServiceComponent : IComponent
{
    public IViewService instance;
}

[Service, Unique]
public sealed class GridServiceComponent : IComponent
{
    public IGridService instance;
}