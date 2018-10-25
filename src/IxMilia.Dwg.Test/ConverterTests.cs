using System;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class ConverterTests
    {
        [Fact]
        public void IntsToJulianDateTime()
        {
            // from AutoDesk spec: 2451544.91568287 = 31 December 1999, 9:58:35PM
            var expected = new DateTime(1999, 12, 31, 9 + 12, 58, 35);
            var actual = Converters.JulianDate(Tuple.Create(2451544, 91568287));
        }
    }
}
