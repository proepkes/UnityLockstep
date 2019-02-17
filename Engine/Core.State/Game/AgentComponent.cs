using System.Collections.Generic;
using BEPUutilities;
using Entitas;
using FixMath.NET;

namespace Lockstep.Core.State.Game
{
    public struct Line
    {
        public Vector2 direction;
        public Vector2 point;
    }

    public class AgentComponent : IComponent
    {
        public Vector2 preferredVelocity;
        public Vector2 velocity;
        public Fix64 maxSpeed;

        public Fix64 neighborDist;
        public int maxNeighbors;

        public Fix64 timeHorizon;

        public IList<Line> orcaLines;
    }
}