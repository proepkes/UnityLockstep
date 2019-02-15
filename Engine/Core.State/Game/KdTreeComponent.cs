using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Lockstep.Core.State.Game
{
    [Unique]
    public class KdTreeComponent : IComponent
    {
        public bool Dirty;
        public AgentTreeNode[] AgentTree;

        public void QueryNeighbors()
        {

        }
    }
}