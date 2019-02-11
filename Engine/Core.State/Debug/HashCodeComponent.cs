using Entitas;

namespace Lockstep.Core.State.Debug
{
    [Debug]
    public sealed class HashCodeComponent : IComponent
    {
        public long value;
    }
}