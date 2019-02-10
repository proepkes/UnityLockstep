using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Lockstep.Core.State.Snapshot
{                 
    [Snapshot]
    public class TickComponent : IComponent
    {
        [PrimaryEntityIndex]
        public uint value;
    }
}
