using System.Collections.Generic;
using System.Linq;

namespace Lockstep.Game.Services.Defaults
{
    public class DefaultSnapshotIndexService : ISnapshotIndexService
    {
        private readonly List<uint> _snapShotIndices = new List<uint>();

        public void AddIndex(uint value)
        {
            _snapShotIndices.Add(value);
        }

        public void RemoveIndex(uint value)
        {
            if (_snapShotIndices.Contains(value))
            {         
                _snapShotIndices.Remove(value);
            }
        }

        public uint GetFirstIndexBefore(uint value)
        {
            return _snapShotIndices.Any() ? _snapShotIndices.Where(index => index <= value).Max() : 0;
        }
    }
}
