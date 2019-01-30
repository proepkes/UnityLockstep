using System.Collections.Generic;   

namespace Lockstep.Core.Interfaces
{
    public interface IStorageService : IService
    {                                                 
        void RegisterChange(uint tick, List<uint> entities);
                                
        void RemoveChanges(uint at);      

        IEnumerable<uint> GetChanges(uint minTick);
    }
}
