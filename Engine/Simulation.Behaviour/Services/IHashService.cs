using System.Collections.Generic;
using Lockstep.Core.Services;

namespace Simulation.Behaviour.Services
{
    public interface IHashService : IService
    {
        long CalculateHashCode(IEnumerable<GameEntity> hashableEntities);

        long CalculateHashCode(GameEntity entity);
    }
}