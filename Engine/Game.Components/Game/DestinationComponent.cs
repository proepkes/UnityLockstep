using BEPUutilities;
using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Lockstep.Core.Components.Game
{
    [Game, Event(EventTarget.Self)]
    public class DestinationComponent : IComponent
    {
        public Vector2 value;
    }
}