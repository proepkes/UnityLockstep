using System.Collections.Generic;
using Lockstep.Core.Services;

namespace Lockstep.Game.Services
{
    public class DefaultHashService : IHashService
    {
        public long CalculateHashCode(IEnumerable<GameEntity> hashableEntities)
        {
            long hashCode = 0;
            foreach (var entity in hashableEntities)
            {
                hashCode ^= CalculateHashCode(entity);         
            }
            return hashCode;
        }

        public long CalculateHashCode(GameEntity entity)
        {
            long hashCode = 0; 
            hashCode ^= entity.position.value.X.RawValue;
            hashCode ^= entity.position.value.Y.RawValue;      
            return hashCode;
        }
    }
}
