using System.Collections.Generic;
using System.Linq;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.DefaultServices
{
    public class DefaultStorageService : IStorageService
    {                                 
        //Mapping: tick -> ids of backup-entities for changed entities
        private readonly Dictionary<uint, List<uint>> _changed = new Dictionary<uint, List<uint>>(); 

        public void RegisterChange(uint tick, List<uint> entities)
        {
            _changed.Add(tick, entities);
        }

        public void RemoveChanges(uint at)
        {
            _changed.Remove(at);
        }
                  
        /// <summary>
        /// Returns the first occurence of all items in the change-buffer at or after the given tick
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public IEnumerable<uint> GetChanges(uint @from)
        {   
            var result = new List<uint>();
            foreach (var e in _changed.Where(pair => pair.Key >= @from).SelectMany(pair => pair.Value))
            {
                if (!result.Contains(e))
                {
                    result.Add(e);
                }    
            }

            return result;
        }
    }
}
