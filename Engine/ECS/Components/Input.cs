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