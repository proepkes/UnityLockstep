using System.Collections.Generic;
using Lockstep.Core.Data;

namespace Lockstep.Core.Interfaces
{
    public interface IWorld
    {
        ServiceContainer Services { get; }
        int EntitiesInCurrentTick { get; }

        TickId CurrentTick { get; }  
        
        void Initialize(byte playerId);

        void AddInput(TickId tickId, Dictionary<PlayerId, List<ICommand>> input);

        void Tick();

        void RevertToTick(uint tick);
    }
}