using Entitas;
using Entitas.CodeGeneration.Attributes;
using FixMath.NET;
using Supercluster.KDTree;

namespace Lockstep.Core.State.Game
{             
    [Unique]
    public class KdTreeComponent : IComponent
    {
        public KDTree<Fix64, GameEntity> value;
    }
}