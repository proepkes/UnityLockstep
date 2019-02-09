using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Simulation.State.Game
{
    [Game] 
    public sealed class LocalIdComponent : IComponent
    {    
        [PrimaryEntityIndex]
        public uint value;
    }
}