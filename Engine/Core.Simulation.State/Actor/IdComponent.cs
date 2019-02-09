using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Lockstep.Core.Components.Actor
{
    [Actor] 
    public sealed class IdComponent : IComponent
    {
        [PrimaryEntityIndex]
        public byte value;
    }
}