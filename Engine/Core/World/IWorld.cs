using System.Collections.Generic;
using Lockstep.Core.Services;

namespace Lockstep.Core.World
{
    public interface IWorld
    {
        uint CurrentTick { get; }

        ServiceContainer Services { get; }

        int ActiveEntities { get; }

        /// <summary>
        /// Used to add input for the world by adding components to the returned entity.
        /// </summary>
        /// <returns></returns>
        InputEntity CreateInputEntity();

        void Initialize(IEnumerable<byte> allActorIds);

        /// <summary>
        /// Predicts one tick. The first predict after a Simulation-tick will create a snapshot for later rollback.
        /// </summary>
        void Predict();

        /// <summary>
        /// Simulates one tick. Call this in case the input from all actors will no longer change for the current tick.
        /// </summary>
        void Simulate();

        void RevertToTick(uint tick);
    }
}