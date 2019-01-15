using ECS.Services;
using Entitas;
using Entitas.CodeGeneration.Attributes;

[Service, Unique]
public sealed class CommandServiceComponent : IComponent
{
    public ICommandService instance;
}