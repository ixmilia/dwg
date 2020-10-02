using System.Linq;
using IxMilia.Dwg.Objects;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class DimStyleTests
    {
        protected static void AssertEquivalent<T>(T expected, T actual)
        {
            foreach (var field in typeof(T).GetFields())
            {
                var expectedField = field.GetValue(expected);
                var actualField = field.GetValue(actual);
                Assert.Equal(expectedField, actualField);
            }

            foreach (var property in typeof(T).GetProperties())
            {
                var expectedProperty = property.GetValue(expected);
                var actualProperty = property.GetValue(actual);
                Assert.Equal(expectedProperty, actualProperty);
            }
        }

        [Fact]
        public void NoDimStyleDifferenceGeneratesNullXData()
        {
            var primary = new DwgDimStyle();
            var modified = new DwgDimStyle();
            var xdata = DwgDimStyle.GenerateStyleDifferenceAsXData(primary, modified);
            Assert.Null(xdata);
        }

        [Fact]
        public void DimStyleDifferenceXDataHasWellKnownName()
        {
            var primary = new DwgDimStyle();
            var modified = new DwgDimStyle()
            {
                DimensionUnitToleranceDecimalPlaces = (short)(primary.DimensionUnitToleranceDecimalPlaces + 1)
            };

            var items = DwgDimStyle.GenerateStyleDifferenceAsXData(primary, modified);
            Assert.Equal("DSTYLE", ((DwgXDataString)items.First()).Value);
        }

        [Fact]
        public void DimStyleDifferenceXDataOnSinglePropertyDifference()
        {
            var primary = new DwgDimStyle();
            var modified = new DwgDimStyle()
            {
                DimensionUnitToleranceDecimalPlaces = (short)(primary.DimensionUnitToleranceDecimalPlaces + 1)
            };

            var diffItems = DwgDimStyle.GenerateStyleDifferenceAsXData(primary, modified);
            Assert.Equal(2, diffItems.Count);

            Assert.Equal("DSTYLE", ((DwgXDataString)diffItems[0]).Value);

            var list = (DwgXDataItemList)diffItems[1];
            Assert.Equal(2, list.Count);
            Assert.Equal(271, ((DwgXDataShort)list[0]).Value);
            Assert.Equal(modified.DimensionUnitToleranceDecimalPlaces, ((DwgXDataShort)list[1]).Value);
        }

        [Fact]
        public void DimStyleDifferenceXDataOnMultiplePropertyDifferences()
        {
            var primary = new DwgDimStyle();
            var modified = new DwgDimStyle()
            {
                DimensioningSuffix = "non-standard-suffix",
                DimensionUnitToleranceDecimalPlaces = (short)(primary.DimensionUnitToleranceDecimalPlaces + 1)
            };

            var diffItems = DwgDimStyle.GenerateStyleDifferenceAsXData(primary, modified);
            Assert.Equal(2, diffItems.Count);

            Assert.Equal("DSTYLE", ((DwgXDataString)diffItems[0]).Value);

            var list = (DwgXDataItemList)diffItems[1];
            Assert.Equal(4, list.Count);

            Assert.Equal(3, ((DwgXDataShort)list[0]).Value);
            Assert.Equal("non-standard-suffix", ((DwgXDataString)list[1]).Value);

            Assert.Equal(271, ((DwgXDataShort)list[2]).Value);
            Assert.Equal(modified.DimensionUnitToleranceDecimalPlaces, ((DwgXDataShort)list[3]).Value);
        }

        [Fact]
        public void DimStyleDiffernceAfterClone()
        {
            var primary = new DwgDimStyle();
            var secondary = primary.Clone();
            secondary.DimensionUnitToleranceDecimalPlaces = 5;

            var diffItems = DwgDimStyle.GenerateStyleDifferenceAsXData(primary, secondary);

            Assert.Equal(2, diffItems.Count);

            Assert.Equal("DSTYLE", ((DwgXDataString)diffItems[0]).Value);

            var list = (DwgXDataItemList)diffItems[1];
            Assert.Equal(2, list.Count);
            Assert.Equal(271, ((DwgXDataShort)list[0]).Value);
            Assert.Equal(5, ((DwgXDataShort)list[1]).Value);
        }

        [Fact]
        public void DimStyleFromCustomXData()
        {
            var primary = new DwgDimStyle();
            var secondary = new DwgDimStyle()
            {
                DimensionUnitToleranceDecimalPlaces = 5
            };

            // sanity check that the values are different
            Assert.NotEqual(primary.DimensionUnitToleranceDecimalPlaces, secondary.DimensionUnitToleranceDecimalPlaces);

            // rebuild dim style from primary with xdata difference; result should equal secondary
            var xdata = DwgDimStyle.GenerateStyleDifferenceAsXData(primary, secondary);
            Assert.True(primary.TryGetStyleFromXDataDifference(xdata, out var reBuiltStyle));
            AssertEquivalent(secondary, reBuiltStyle);
        }

        [Fact]
        public void DimStyleGetVariable()
        {
            var style = new DwgDimStyle()
            {
                DimensionUnitToleranceDecimalPlaces = 5
            };
            Assert.Equal((short)5, style.GetVariable("DIMDEC"));
        }

        [Fact]
        public void DimStyleSetVariable()
        {
            var style = new DwgDimStyle();
            Assert.NotEqual(5, style.DimensionUnitToleranceDecimalPlaces);
            style.SetVariable("DIMDEC", (short)5);
            Assert.Equal(5, style.DimensionUnitToleranceDecimalPlaces);
        }
    }
}
