using Entitas;

namespace Simulation.State.Input
{
    [Input]
    public class SelectionComponent : IComponent
    {
        public uint[] entityIds;     
    }
}