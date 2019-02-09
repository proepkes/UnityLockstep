using Entitas;

namespace Lockstep.Core.Components.Game
{
    [Game]
    public sealed class HealthComponent : IComponent
    {
        public int value;
    }
}