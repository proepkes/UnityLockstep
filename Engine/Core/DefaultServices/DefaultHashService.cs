using System.Collections.Generic;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.DefaultServices
{
    public class DefaultHashService : IHashService
    {
        public long CalculateHashCode(IEnumerable<GameEntity> entities)
        {
            long hashCode = 0;
            foreach (var entity in entities)
            {
                hashCode ^= entity.position.value.X.RawValue;
                hashCode ^= entity.position.value.Y.RawValue;
            }
            return hashCode;
        }
    }
}
