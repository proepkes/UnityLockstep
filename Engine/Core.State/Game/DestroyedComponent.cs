using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Lockstep.Core.State.Game
{
    [Cleanup(CleanupMode.DestroyEntity)]
    public sealed class DestroyedComponent : IComponent
    {
    }
}
