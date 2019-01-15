using BEPUutilities;
using ECS.Data;
using Entitas;
using Entitas.CodeGeneration.Attributes;

[Input, Unique]
public class FrameComponent : IComponent
{
    public Command[] Commands;
}

[Input]
public class SelectCommandComponent : IComponent
{
    public int[] EntityIds;
}

[Input]
public class NavigationCommandComponent : IComponent
{
    public Vector2 Destination;
}

