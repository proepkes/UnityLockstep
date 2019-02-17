using BEPUutilities;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using FixMath.NET;
using Lockstep.Core.State.Game.KdTree;

namespace Lockstep.Core.State.Game
{             
    [Unique]
    public class KdTreeComponent : IComponent
    {
        public KdTree<Fix64, GameEntity> value;
    }
}