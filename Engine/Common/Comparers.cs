using System;
using System.Collections.Generic;

namespace Lockstep.Common
{
    public sealed class EnumComparer<TEnum> : IEqualityComparer<TEnum> where TEnum : struct, IComparable
    {                                                                               
        public bool Equals(TEnum x, TEnum y)
        {
            return x.GetHashCode() == y.GetHashCode();
        }
                                                                               
        public int GetHashCode(TEnum obj)
        {
            return obj.GetHashCode();
        }
    }
}