using Entitas;
using Entitas.CodeGeneration.Attributes;

[Input]
[Unique]
public class TickComponent : IComponent
{
    public int value;
}