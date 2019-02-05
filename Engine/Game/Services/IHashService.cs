using System.Collections.Generic;
using Lockstep.Core.Services;

namespace Lockstep.Game.Services
{
    public interface IHashService : IService
    {
        long CalculateHashCode(IEnumerable<GameEntity> hashableEntities);

        long CalculateHashCode(GameEntity entity);
    }
}