using Entitas;
using Entitas.CodeGeneration.Attributes;
using FixMath.NET;
using Lockstep.Core.State.Game.KdTree;
using Supercluster.KDTree;

namespace Lockstep.Core.State.Game
{             
    [Unique]
    public class KdTreeComponent : IComponent
    {
        public KdTree<Fix64, GameEntity> value;
    }
}