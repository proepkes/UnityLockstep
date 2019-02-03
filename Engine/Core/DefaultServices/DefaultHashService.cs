using System.Collections.Generic;
using System.Linq;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.DefaultServices
{
    public class DefaultHashService : IHashService
    {       
        public long CalculateHashCode(IEnumerable<GameEntity> hashableEntities)
        {
            long hashCode = 0;
            foreach (var entity in hashableEntities)
            {
                hashCode ^= entity.position.value.X.RawValue;
                hashCode ^= entity.position.value.Y.RawValue;                 
            }
            return hashCode;
        }
    }
}
