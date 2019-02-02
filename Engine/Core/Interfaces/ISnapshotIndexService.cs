namespace Lockstep.Core.Interfaces
{
    /// <summary>
    /// A service to manage snapshot-indices
    /// </summary>
    public interface ISnapshotIndexService : IService
    {
        void AddIndex(uint value);

        uint GetFirstIndexBefore(uint value);
    }
}
