using System;
using System.Collections.Generic;

namespace ppcg.math {
    public static class MyMath {

        private const int INT_BITS = sizeof(int) * 8 - 1;

        private static HashSet<Type> NumericTypes = new HashSet<Type> {
            typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong),
            typeof(float), typeof(double), typeof(decimal)
        };

        /**
         * https://graphics.stanford.edu/~seander/bithacks.html#IntegerAbs
         **/
        public static int Abs(this int v) {
            int mask = v >> INT_BITS;
            return (v + mask) ^ mask;
        }

        public static bool IsNumeric(this Type type) {
            return NumericTypes.Contains(type) || NumericTypes.Contains(Nullable.GetUnderlyingType(type));
        }

        public static int BinarySearch(this int[] source, int value) {
            int start = 0;
            int end = source.Length;
            int mid = end >> 1;

            while(mid != end) {
                if(source[mid] < value) {
                    start = mid;
                    mid = (start + end + 1) >> 1;
                } else {
                    end = mid;
                    mid = (start + end) >> 1;
                }
            }

            return mid;
        }

    }
}
