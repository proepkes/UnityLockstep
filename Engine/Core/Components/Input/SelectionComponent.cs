using Entitas;
using Lockstep.Core.Data;

namespace Lockstep.Core.Components.Input
{
    [Input]
    public class SelectionComponent : IComponent
    {
        public EntityId[] values;     
    }
}