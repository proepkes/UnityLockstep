using System.Collections.Generic;

namespace Lockstep.Core.Interfaces
{
    public interface IHashService : IService
    {  
        long CalculateHashCode(IEnumerable<GameEntity> hashableEntities, GameStateContext context, ILogService logger);
    }
}