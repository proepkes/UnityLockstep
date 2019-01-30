using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Lockstep.Core.Components.Game
{
    [Game] 
    public sealed class IdComponent : IComponent
    {
        [PrimaryEntityIndex]
        public uint value;
    }
}