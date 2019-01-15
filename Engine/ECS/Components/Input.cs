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
public class NavigateCommandComponent : IComponent
{
    public int[] EntityIds;
    public Vector2 Destination;
}

