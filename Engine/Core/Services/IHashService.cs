using System.Collections.Generic;

namespace Lockstep.Core.Services
{
    public interface IHashService : IService
    {
        long CalculateHashCode(IEnumerable<GameEntity> hashableEntities);

        long CalculateHashCode(GameEntity entity);
    }
}