using System;

namespace IxMilia.Dwg
{
    internal static class Converters
    {
        private static DateTime JulianDublinBase = new DateTime(1899, 12, 31, 0, 0, 0);

        public static bool BoolByte(byte value)
        {
            return value != 0;
        }

        public static bool BoolShort(short value)
        {
            return value != 0;
        }

        public static T BoolToEnum<T>(bool value) where T : struct
        {
            return default(T);
        }

        public static DwgPoint DoublePoint(Tuple<double, double> value)
        {
            return new DwgPoint(value.Item1, value.Item2, 0.0);
        }

        public static DwgPoint TriplePoint(Tuple<double, double, double> value)
        {
            return new DwgPoint(value.Item1, value.Item2, value.Item3);
        }

        public static DwgVector TripleVector(Tuple<double, double, double> value)
        {
            return new DwgVector(value.Item1, value.Item2, value.Item3);
        }

        public static DateTime JulianDate(Tuple<int, int> value)
        {
            var sinceDublin = TimeSpan(value);
            return JulianDublinBase.Add(sinceDublin);
        }

        public static TimeSpan TimeSpan(Tuple<int, int> value)
        {
            var days = System.TimeSpan.FromDays(value.Item1);
            var msIntoDay = System.TimeSpan.FromMilliseconds(value.Item2);
            return days.Add(msIntoDay);
        }

        public static void SetFlag(ref int flags, int mask)
        {
            flags |= mask;
        }

        public static void ClearFlag(ref int flags, int mask)
        {
            flags &= ~mask;
        }

        public static bool GetFlag(int flags, int mask)
        {
            return (flags & mask) != 0;
        }

        public static void SetFlag(bool value, ref int flags, int mask)
        {
            if (value) SetFlag(ref flags, mask);
            else ClearFlag(ref flags, mask);
        }
    }
}
