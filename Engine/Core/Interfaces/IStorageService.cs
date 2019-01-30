using System.Collections.Generic;   

namespace Lockstep.Core.Interfaces
{
    public interface IStorageService : IService
    {                                                 
        void RegisterChange(uint tick, List<GameEntity> entities);
                                
        void RemoveChanges(uint at);      

        IEnumerable<GameEntity> GetChanges(uint minTick);
    }
}
