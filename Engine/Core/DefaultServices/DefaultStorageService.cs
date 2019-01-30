using System.Collections.Generic;
using System.Linq;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.DefaultServices
{
    public class DefaultStorageService : IStorageService
    {                                                                                                
        private readonly Dictionary<uint, List<GameEntity>> _changed = new Dictionary<uint, List<GameEntity>>();
             

        public void RegisterChange(uint tick, List<GameEntity> entities)
        {
            _changed.Add(tick, entities);
        }

        public void RemoveChanges(uint at)
        {
            _changed.Remove(at);
        }
                  
        /// <summary>
        /// Returns the first occurence of all entities in the change-buffer at or after the given tick
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public IEnumerable<GameEntity> GetChanges(uint @from)
        {   
            var result = new Dictionary<uint, GameEntity>();
            foreach (var e in _changed.Where(pair => pair.Key >= @from).SelectMany(pair => pair.Value))
            {
                if (!result.ContainsKey(e.idReference.value))
                {
                    result.Add(e.idReference.value, e);
                }    
            }

            return result.Select(pair => pair.Value);
        }
    }
}
