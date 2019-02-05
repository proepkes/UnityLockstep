using Entitas;

namespace Lockstep.Core.Components.Input
{
    [Input]
    public class SelectionComponent : IComponent
    {
        public uint[] entityIds;     
    }
}