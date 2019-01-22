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
public class NavigationInputDataComponent : IComponent
{
    public int[] EntityIds;

    public Vector2 Destination;
}

[Input]
public class SpawnInputDataComponent : IComponent
{                    
    public int OwnerId;

    public int EntityConfigId;

    public Vector2 Position;
}