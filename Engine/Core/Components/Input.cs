using BEPUutilities;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using Lockstep.Core.Data;

[Input, Unique]
public class FrameComponent : IComponent
{
    public Frame value;
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