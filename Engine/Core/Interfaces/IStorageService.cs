using System.Collections.Generic;   

namespace Lockstep.Core.Interfaces
{
    public interface IStorageService : IService
    {
        void RegisterNew(uint tick, List<uint> ids);

        void RegisterChange(uint tick, List<GameEntity> entities);
                                
        void RemoveChanges(uint at);

        void RemoveNewEntites(uint at);

        IEnumerable<uint> GetAllNew(uint minTick);
    }
}
