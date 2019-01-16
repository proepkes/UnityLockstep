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
public class InputPositionComponent : IComponent
{
    public Vector2 value;
}

[Input]
public class GameEntityIdsComponent : IComponent
{
    public int[] value;
}

