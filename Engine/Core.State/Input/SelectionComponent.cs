using Entitas;

namespace Lockstep.Core.State.Input
{
    [Input]
    public class SelectionComponent : IComponent
    {
        public uint[] entityIds;     
    }
}