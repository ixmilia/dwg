using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgSolid3D
    {
        public List<byte> RawData { get; private set; }
        public List<byte> TrailingData { get; private set; }
        public int FinalBitCount { get; set; }
        public byte FinalByte { get; set; }

        internal override void WriteSpecific(BitWriter writer, DwgVersionId version)
        {
            DwgRegion.WriteSpecificRegionData(writer, RawData, TrailingData, FinalBitCount, FinalByte, ref _objectSize);
        }

        internal override void ParseSpecific(BitReader reader, int objectBitOffsetStart, DwgVersionId version)
        {
            DwgRegion.ParseSpecificRegionData(reader, objectBitOffsetStart, RawData, TrailingData, _objectSize, out var finalBitCount, out var finalByte);
            FinalBitCount = finalBitCount;
            FinalByte = finalByte;
            AssertObjectSize(reader, objectBitOffsetStart);
        }
    }
}
