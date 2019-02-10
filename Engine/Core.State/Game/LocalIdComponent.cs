using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Lockstep.Core.State.Game
{
    [Game] 
    public sealed class LocalIdComponent : IComponent
    {    
        [PrimaryEntityIndex]
        public uint value;
    }
}