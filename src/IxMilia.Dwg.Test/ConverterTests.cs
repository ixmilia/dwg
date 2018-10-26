using System;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class ConverterTests
    {
        public const long MSPerDay = 24 * 60 * 60 * 1000; // hours/day * minutes/hour * seconds/minute * ms/second

        // from AutoDesk spec: 2451544.91568287 = 31 December 1999, 9:58:35PM
        public const double AutoDeskStandardDouble = 2451544.91568287;
        public static DateTime AutoDeskStandardDateTime = new DateTime(1999, 12, 31, 9 + 12, 58, 35);
        public static int AutoDeskStandardDay = (int)Math.Truncate(AutoDeskStandardDouble);
        public static int AutoDeskStandardMSIntoDay = (int)((AutoDeskStandardDouble - Math.Truncate(AutoDeskStandardDouble)) * MSPerDay) + 1; // this +1 helps with rounding errors; not relevant to the actual conversion

        [Fact]
        public void IntsToJulianDateTime()
        {
            var expected = AutoDeskStandardDateTime;
            var actual = Converters.JulianDate(Tuple.Create(AutoDeskStandardDay, AutoDeskStandardMSIntoDay));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void JulianDateTimeToInts()
        {
            var expected = Tuple.Create(AutoDeskStandardDay, AutoDeskStandardMSIntoDay);
            var actual = Converters.JulianDate(AutoDeskStandardDateTime);
            Assert.Equal(expected, actual);
        }
    }
}
