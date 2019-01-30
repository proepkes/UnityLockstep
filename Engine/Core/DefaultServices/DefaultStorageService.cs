using System.Collections.Generic;
using System.Linq;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.DefaultServices
{
    public class DefaultStorageService : IStorageService
    {
        private readonly Dictionary<uint, List<uint>> _added = new Dictionary<uint, List<uint>>();
        private readonly Dictionary<uint, List<GameEntity>> _changed = new Dictionary<uint, List<GameEntity>>();

        public void RegisterNew(uint tick, List<uint> ids)
        {
            _added.Add(tick, ids);
        }

        public void RegisterChange(uint tick, List<GameEntity> entities)
        {
            _changed.Add(tick, entities);
        }

        public void RemoveChanges(uint at)
        {
            _changed.Remove(at);
        }

        public void RemoveNewEntites(uint at)
        {
            _added.Remove(at);
        }

        public IEnumerable<uint> GetAllNew(uint @from)
        {                               
            return _added.Where(pair => pair.Key >= from).SelectMany(pair => pair.Value);
        }
    }
}
