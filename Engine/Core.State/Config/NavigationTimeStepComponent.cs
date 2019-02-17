using Entitas;
using Entitas.CodeGeneration.Attributes;
using FixMath.NET;

namespace Lockstep.Core.State.Config
{
    [Unique, Config]
    public class NavigationTimeStepComponent : IComponent
    {
        public Fix64 value;
    }
}
