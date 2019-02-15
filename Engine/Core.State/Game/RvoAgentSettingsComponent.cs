using System.Collections.Generic;
using BEPUutilities;
using Entitas;
using FixMath.NET;

namespace Lockstep.Core.State.Game
{
    public class RvoAgentSettingsComponent : IComponent
    {
        public Vector2 preferredVelocity;
        public Fix64 neighborDist;
        public int maxNeighbors;
        public IList<KeyValuePair<Fix64, GameEntity>> agentNeighbors;
    }
}