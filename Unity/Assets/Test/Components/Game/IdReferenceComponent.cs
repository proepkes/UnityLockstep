using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Lockstep.Core.Components.Game
{
    //Entities with this component store changes of other entities from the past
    [Game]
    public class IdReferenceComponent : IComponent
    {
        [PrimaryEntityIndex]
        public uint internalId;

        public uint referenceId;

        public uint tick;
    }
}
