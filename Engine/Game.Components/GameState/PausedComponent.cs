using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Lockstep.Core.Components.GameState
{
    [GameState, Unique]
    public class PausedComponent : IComponent
    {
    }
}