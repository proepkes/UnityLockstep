using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Lockstep.Core.State.GameState
{
    [GameState, Unique]
    public sealed class HashCodeComponent : IComponent
    {
        public long value;
    }
}