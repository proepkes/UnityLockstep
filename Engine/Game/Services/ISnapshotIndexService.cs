using Lockstep.Core.Services;

namespace Lockstep.Game.Services
{
    /// <summary>
    /// A service to manage snapshot-indices
    /// </summary>
    public interface ISnapshotIndexService : IService
    {
        void AddIndex(uint value);

        void RemoveIndex(uint value);

        uint GetFirstIndexBefore(uint value);
    }
}
