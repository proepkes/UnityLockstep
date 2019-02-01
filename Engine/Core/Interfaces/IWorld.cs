using System.Collections.Generic;   

namespace Lockstep.Core.Interfaces
{
    public interface IWorld
    {
        Services Services { get; }

        int EntitiesInCurrentTick { get; }

        uint CurrentTick { get; }  
        
        void Initialize(byte playerId);

        void AddInput(uint tickId, byte player, List<ICommand> input);

        void Predict();

        void Simulate();

        void RevertToTick(uint tick);
    }
}