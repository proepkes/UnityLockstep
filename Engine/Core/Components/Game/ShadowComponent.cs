using Entitas;                          
using Lockstep.Core.Data;

namespace Lockstep.Core.Components.Game
{
    //Entities with this component store changes of other entities from the past
    [Game]
    public class ShadowComponent : IComponent
    {                      
        public EntityId entityId;

        public TickId tick;      
    }
}
