using Entitas;

namespace Lockstep.Core.State.Snapshot
{
    [Snapshot]
    public sealed class HashCodeComponent : IComponent
    {
        public long value;
    }
}