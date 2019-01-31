using Entitas;
using Entitas.CodeGeneration.Attributes;    

namespace Lockstep.Core.Components.GameState
{
    [GameState, Unique]
    public class TickComponent : IComponent
    {                         
        public uint value;
    }     
    
}