using BEPUphysics.Entities;
using FixMath.NET;

namespace Lockstep.Framework
{
    public interface ILockstepEntity
    {
        ulong ID { get; set; }

        string GetAssetName();

        ulong GetHashCode();
    }
}