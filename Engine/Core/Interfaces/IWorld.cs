using System.Collections.Generic;   

namespace Lockstep.Core.Interfaces
{
    public interface IWorld
    {
        ServiceContainer Services { get; }
        int EntitiesInCurrentTick { get; }

        uint CurrentTick { get; }  
        
        void Initialize(byte playerId);

        void AddInput(uint tickId, Dictionary<byte, List<ICommand>> input);

        void Tick();

        void RevertToTick(uint tick);
    }
}