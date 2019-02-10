using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Lockstep.Core.State.GameState
{
    [GameState, Unique]
    public class PredictingComponent : IComponent
    {
    }
}
