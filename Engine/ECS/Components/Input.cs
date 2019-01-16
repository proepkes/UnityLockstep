using BEPUutilities;
using ECS.Data;
using Entitas;
using Entitas.CodeGeneration.Attributes;

[Input, Unique]
public class FrameComponent : IComponent
{
    public SerializedInput[] SerializedInputs;
}         

[Input]
public class MousePositionComponent : IComponent
{
    public Vector2 value;
}

[Input]
public class GameEntityIdsComponent : IComponent
{
    public int[] value;
}

[Input]
public class NavigationInputComponent : IComponent
{

}