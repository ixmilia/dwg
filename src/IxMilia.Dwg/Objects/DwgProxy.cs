#nullable enable

namespace IxMilia.Dwg.Objects
{
    public partial class DwgProxy
    {
        public DwgObject? Parent { get; set; }

        public byte[] RawData { get; set; }
        public byte TrailingBits { get; set; }
        public int TrailingBitCount { get; set; }

        internal override void ParseSpecific(BitReader reader, int objectBitOffsetStart, DwgVersionId version)
        {
            var consumedBits = reader.BitOffset - objectBitOffsetStart;
            var totalRemainingBits = _objectSize - consumedBits;
            var remainingBytes = totalRemainingBits / 8;
            TrailingBitCount = totalRemainingBits % 8;
            RawData = reader.ReadBytes(remainingBytes);
            TrailingBits = (byte)reader.ReadBits(TrailingBitCount);

            AssertObjectSize(reader, objectBitOffsetStart);

            // handles
            _parentHandleReference = reader.Read_H();
            for (int i = 0; i < _reactorCount; i++)
            {
                _reactorHandleReferences.Add(reader.Read_H());
            }

            _xDictionaryObjectHandleReference = reader.Read_H();
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            if (_parentHandleReference.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect parent handle reference code");
            }

            Parent = objectCache.GetObject(reader, ResolveHandleReference(_parentHandleReference), allowNull: true);
        }

        internal override void OnBeforeObjectWrite(DwgVersionId version)
        {
            _parentHandleReference = Parent is { }
                ? Parent.MakeHandleReference(DwgHandleReferenceCode.SoftPointer)
                : DwgHandleReference.Empty;
        }

        internal override void WriteSpecific(BitWriter writer, DwgVersionId version)
        {
            writer.WriteBytes(RawData);
            writer.WriteBits(TrailingBits, TrailingBitCount);
            _objectSize = writer.BitCount;

            writer.Write_H(_parentHandleReference);
            for (int i = 0; i < _reactorCount; i++)
            {
                writer.Write_H(_reactorHandleReferences[i]);
            }

            writer.Write_H(_xDictionaryObjectHandleReference);
        }
    }
}
