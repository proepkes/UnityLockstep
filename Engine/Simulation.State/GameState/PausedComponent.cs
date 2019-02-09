using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Simulation.State.GameState
{
    [GameState, Unique]
    public class PausedComponent : IComponent
    {
    }
}