using Entitas;
using Entitas.CodeGeneration.Attributes;
using Lockstep.Core.Data;

namespace Lockstep.Core.Components.Game
{
    [Game] 
    public sealed class IdComponent : IComponent
    {    
        [PrimaryEntityIndex]
        public EntityId value;
    }
}