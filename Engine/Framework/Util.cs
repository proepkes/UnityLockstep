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
            return ReferenceEquals(obj, null) == false;
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
            return (int)(f1 + Fix64.One - 1);
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

        public static int CeilToInt(this Fix64 f1)
        {
            return (int)((f1 + Fix64.One - 1).RawValue >> 32);
        }        

        public static Vector2 Lerped(this Vector2 v, Vector2 end, Fix64 interpolationAmount)
        {                
            var startAmount = F64.C1 - interpolationAmount;

            var result = v;
            result.X = v.X * startAmount + end.X * interpolationAmount;
            result.Y = v.Y * startAmount + end.Y * interpolationAmount;
            return result;
        }

        public static Vector2 Normalize(Vector2 v, out Fix64 magnitude)
        {
            magnitude = v.Length();
            // This is the same constant that Unity uses
            if (magnitude > Fix64.Zero)
            {
                return v / magnitude;
            }

            return Vector2.Zero;
        }
        public static Vector2 ClampMagnitude(Vector2 vector, Fix64 maxLength)
        {
            if (vector.LengthSquared() > maxLength * maxLength)
                return  Vector2.Normalize(vector) * maxLength;
            return vector;
        }

        public static Vector2 ToVector2(this Vector3 v)
        {                                                             
            return new Vector2(v.X, v.Z);
        }

        public static Vector3 ToVector3(this Vector2 v)
        {
            return new Vector3(v.X, 0, v.Y);
        }    
    }

}
