using Entitas;
using Entitas.CodeGeneration.Attributes;
using Lockstep.Core.Data;

namespace Lockstep.Core.Components.GameState
{
    [GameState, Unique]
    public class TickComponent : IComponent
    {                         
        public TickId value;
    }     
    
}