using BEPUphysics.Entities;
using FixMath.NET;

namespace Lockstep.Framework
{
    public interface ILockstepAgent : ILockstepEntity
    {

        Entity Body { get; }

        Fix64 Radius { get; }

    }
}