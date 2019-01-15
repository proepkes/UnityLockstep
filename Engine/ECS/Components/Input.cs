using ECS.Data;
using Entitas;
using Entitas.CodeGeneration.Attributes;
                                            

[Input, Unique]
public class FrameComponent : IComponent
{
    public uint FrameNumber { get; set; }
    public Command[] Commands { get; set; }
}

