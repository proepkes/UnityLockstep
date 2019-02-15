using FixMath.NET;

namespace Lockstep.Core.State
{
    public struct AgentTreeNode
    {
        public int begin_;
        public int end_;
        public int left_;
        public int right_;
        public Fix64 maxX_;
        public Fix64 maxY_;
        public Fix64 minX_;
        public Fix64 minY_;
    }
}