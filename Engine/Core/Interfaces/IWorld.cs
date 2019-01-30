using System.Collections.Generic;

namespace Lockstep.Core.Interfaces
{
    public interface IWorld
    {
        int EntitiesInCurrentTick { get; }  

        uint CurrentTick { get; }  
        
        void Initialize(byte playerId);

        void Tick(Dictionary<byte, List<ICommand>> input);

        void RevertToTick(uint tick);
    }
}