using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Lockstep.Core.Components.Game
{
    [Game] 
    public class IdComponent : IComponent
    {
        [PrimaryEntityIndex]
        public uint value;
    }
}