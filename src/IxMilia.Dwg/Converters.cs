using System;

namespace IxMilia.Dwg
{
    internal static class Converters
    {
        private static DateTime JulianDublinBase = new DateTime(1899, 12, 31, 0, 0, 0);
        private const int JulianDublinOffset = 2415020;

        public static bool BoolByte(byte value)
        {
            return value != 0;
        }

        public static byte BoolByte(bool value)
        {
            return value ? (byte)1 : (byte)0;
        }

        public static bool BoolShort(short value)
        {
            return value != 0;
        }

        public static short BoolShort(bool value)
        {
            return value ? (short)1 : (short)0;
        }

        public static T BoolToEnum<T>(bool value) where T : struct
        {
            return default(T);
        }

        public static DwgPoint DoublePoint(Tuple<double, double> value)
        {
            return new DwgPoint(value.Item1, value.Item2, 0.0);
        }

        public static Tuple<double, double> DoublePoint(DwgPoint p)
        {
            return Tuple.Create(p.X, p.Y);
        }

        public static DwgPoint TriplePoint(Tuple<double, double, double> value)
        {
            return new DwgPoint(value.Item1, value.Item2, value.Item3);
        }

        public static Tuple<double, double, double> TriplePoint(DwgPoint p)
        {
            return Tuple.Create(p.X, p.Y, p.Z);
        }

        public static DwgVector TripleVector(Tuple<double, double, double> value)
        {
            return new DwgVector(value.Item1, value.Item2, value.Item3);
        }

        public static Tuple<double, double, double> TripleVector(DwgVector v)
        {
            return Tuple.Create(v.X, v.Y, v.Z);
        }

        public static DateTime JulianDate(Tuple<int, int> value)
        {
            var dublinOffset = value.Item1 - JulianDublinOffset;
            var date = JulianDublinBase.AddDays(dublinOffset).AddMilliseconds(value.Item2);
            return date;
        }

        public static Tuple<int, int> JulianDate(DateTime value)
        {
            var dublinOffset = value - JulianDublinBase;
            var fractionalDay = dublinOffset.Add(System.TimeSpan.FromDays(-dublinOffset.Days));
            return Tuple.Create(dublinOffset.Days + JulianDublinOffset, (int)fractionalDay.TotalMilliseconds);
        }

        public static TimeSpan TimeSpan(Tuple<int, int> value)
        {
            var days = System.TimeSpan.FromDays(value.Item1);
            var msIntoDay = System.TimeSpan.FromMilliseconds(value.Item2);
            return days.Add(msIntoDay);
        }

        public static Tuple<int, int> TimeSpan(TimeSpan value)
        {
            var days = value.Days;
            var msIntoDay = (value - System.TimeSpan.FromDays(days)).Milliseconds;
            return Tuple.Create(days, msIntoDay);
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
