using System.Collections.Generic;
using System.Linq;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.DefaultServices
{
    public class DefaultHashService : IHashService
    {
        public long CalculateHashCode(IEnumerable<GameEntity> hashableEntities, GameStateContext context, ILogService logger)
        {
            long hashCode = 0;
            foreach (var entity in hashableEntities.OrderBy(entity => entity.localId.value))
            {
                hashCode ^= CalculateHashCode(entity);         
                if (context.tick.value > 60 && context.tick.value < 90 && entity.localId.value == 81)
                {
                    logger.Warn(entity.localId.value + ": " + hashCode);
                }
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
