using System.Collections.Generic;
using Lockstep.Client.Implementations;

namespace Lockstep.Core.Interfaces
{
    public interface IWorld
    {
        GameLog GameLog { get; }

        Services Services { get; }

        int EntitiesInCurrentTick { get; }

        uint CurrentTick { get; }

        void Initialize(byte[] allActorIds);

        void AddInput(uint tickId, byte actorId, List<ICommand> input);

        void Predict();

        void Simulate();

        void RevertToTick(uint tick);
    }
}