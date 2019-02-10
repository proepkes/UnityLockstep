using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Lockstep.Core.State.Actor
{
    [Actor] 
    public sealed class IdComponent : IComponent
    {
        [PrimaryEntityIndex]
        public byte value;
    }
}