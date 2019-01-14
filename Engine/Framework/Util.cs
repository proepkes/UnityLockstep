using BEPUutilities;
using FixMath.NET;

namespace Lockstep.Framework
{
    internal static class Util
    {
        public static bool IsNull(this object obj)
        {
            return ReferenceEquals(obj, null);
        }
        public static bool IsNotNull(this System.Object obj)
        {
            return System.Object.ReferenceEquals(obj, null) == false;
        }

        public static Fix64 ClampOne(this Fix64 f1)
        {
            if (f1 > Fix64.One)
                return Fix64.One;
            if (f1 < -Fix64.One)
                return -Fix64.One;
            return f1;
        }
        public static int CeilToInt(this long f1)
        {
            return (int)((f1 + Fix64.One - 1));
        }
                      
        public static Fix64 Abs(this Fix64 f1)
        {
            return f1 < 0 ? -f1 : f1;
        }

        public static int RoundToInt(this Fix64 f1)
        {
            //Works with negatives!
            return (int)((f1 + Fix64.One/2 - 1).RawValue >> 32);
        }

        public static Fix64 FastDistance(this Vector2 one, Vector2 other)
        {
            var temp1 = one.X - other.X;
            temp1 *= temp1;
            var temp2 = one.Y - other.Y;
            temp2 *= temp2;
            return (temp1 + temp2);
        }

        public static double ToDouble(this Fix64 f1)
        {
            return (f1.RawValue / ((double)Fix64.One));
        }

        public static Fix64 GetLongHashCode(this Vector2 v)
        {
            return v.X * 31 + v.Y * 7;
        }

        public static int GetStateHash(this Vector2 v)
        {
            return (int)(v.GetLongHashCode() % int.MaxValue);
        }

        public static int CeilToInt(this Fix64 f1)
        {
            return (int)((f1 + Fix64.One - 1).RawValue >> 32);
        }
        public static void Normalize(this Vector2 v, out Fix64 mag)
        {                 
            mag = v.Length();
            if (mag == 0)
            {
                return;
            }

            if (mag == Fix64.One)
            {
                return;
            }

            v.Normalize();           
        }
        public static Vector2 Lerped(this Vector2 v, Vector2 end, Fix64 interpolationAmount)
        {                
            var startAmount = F64.C1 - interpolationAmount;

            var result = v;
            result.X = v.X * startAmount + end.X * interpolationAmount;
            result.Y = v.Y * startAmount + end.Y * interpolationAmount;
            return result;
        }

        public static ulong GetLongHashCode(this Vector3 v)
        {
            return (ulong) (long)(v.X * 31 + v.Z * 7);
        }

        public static int GetStateHash(this Vector3 v)
        {
            return (int)(v.GetLongHashCode() % int.MaxValue);
        }
    }

}
