using Entitas;

namespace Lockstep.Core.State.Game
{
    [Game]
    public sealed class HealthComponent : IComponent
    {
        public int value;
    }
}