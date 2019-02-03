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
                hashCode ^= entity.position.value.X.RawValue;
                hashCode ^= entity.position.value.Y.RawValue;
                if (context.tick.value > 71 && context.tick.value < 75) 
                {
                    logger.Warn(entity.localId.value + ": " + hashCode);
                }
            }
            return hashCode;
        }
    }
}
