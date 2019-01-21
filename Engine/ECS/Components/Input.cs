using BEPUutilities;
using ECS.Data;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using FixMath.NET;

[Input, Unique]
public class FrameComponent : IComponent
{
    public SerializedInput[] SerializedInputs;
}

[Input]
public class NavigationInputDataComponent : IComponent
{
    public int[] EntityIds;

    public Vector2 Destination;
}

[Input]
public class SpawnInputDataComponent : IComponent
{
    public int entityConfigId;

    public Vector2 position;
}