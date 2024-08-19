

using System.Collections.Generic;

namespace EasyAnimator
{
 
    public sealed class FastComparer : IEqualityComparer<object>
    {
         

        public static readonly FastComparer Instance = new FastComparer();

        bool IEqualityComparer<object>.Equals(object x, object y) => Equals(x, y);

        int IEqualityComparer<object>.GetHashCode(object obj) => obj.GetHashCode();

         
    }

 
    public sealed class FastReferenceComparer : IEqualityComparer<object>
    {
         

        public static readonly FastReferenceComparer Instance = new FastReferenceComparer();

        bool IEqualityComparer<object>.Equals(object x, object y) => ReferenceEquals(x, y);

        int IEqualityComparer<object>.GetHashCode(object obj) => obj.GetHashCode();

         
    }
}

