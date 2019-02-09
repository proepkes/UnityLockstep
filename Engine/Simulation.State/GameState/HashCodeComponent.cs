using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Simulation.State.GameState
{
    [GameState, Unique]
    public sealed class HashCodeComponent : IComponent
    {
        public long value;
    }
}