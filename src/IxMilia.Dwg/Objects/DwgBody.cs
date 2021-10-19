using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgBody
    {
        public List<byte> RawData { get; private set; }
        public List<byte> TrailingData { get; private set; }

        internal override void WriteSpecific(BitWriter writer, DwgVersionId version)
        {
            DwgRegion.WriteSpecificRegionData(writer, RawData, TrailingData, ref _objectSize);
        }

        internal override void ParseSpecific(BitReader reader, int objectBitOffsetStart, DwgVersionId version)
        {
            DwgRegion.ParseSpecificRegionData(reader, objectBitOffsetStart, RawData, TrailingData, _objectSize);
            AssertObjectSize(reader, objectBitOffsetStart);
        }
    }
}
