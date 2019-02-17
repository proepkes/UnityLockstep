using FixMath.NET;

namespace Lockstep.Core.State.Game.KdTree.Math
{
    public class Fix64Math  : TypeMath<Fix64>
    {
        public override int Compare(Fix64 a, Fix64 b)
        {
            return a.CompareTo(b);
        }

        public override Fix64 MinValue => Fix64.MinValue;
        public override Fix64 MaxValue => Fix64.MaxValue;
        public override Fix64 Multiply(Fix64 a, Fix64 b)
        {
            return a * b;
        }

        public override Fix64 DistanceSquaredBetweenPoints(Fix64[] a, Fix64[] b)
        {        
            Fix64 dist = 0;
            for (var i = 0; i < a.Length; i++)
            {
                dist += (a[i] -b[i]) * (a[i] - b[i]);
            }

            return dist;
        }

        public override Fix64 Zero => Fix64.Zero;
        public override Fix64 NegativeInfinity => Fix64.MinValue;
        public override Fix64 PositiveInfinity => Fix64.MaxValue;
        public override Fix64 Add(Fix64 a, Fix64 b)
        {
            return a + b;
        }

        public override Fix64 Subtract(Fix64 a, Fix64 b)
        {
            return a - b;
        }

        public override bool AreEqual(Fix64 a, Fix64 b)
        {
            return a.Equals(b);
        }
    }
}
