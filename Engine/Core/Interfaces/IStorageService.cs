using System.Collections.Generic;   

namespace Lockstep.Core.Interfaces
{
    public interface IStorageService : IService
    {                                                 
        void RegisterChange(uint tick, List<uint> entities);
                                
        void RemoveChanges(uint at);

        /// <summary>                   
        /// </summary>
        /// <param name="minTick"></param>
        /// <returns>The first occurence of each item in the storage at or after the given tick</returns>
        IEnumerable<uint> GetFirstChangeOccurences(uint minTick);
    }
}
