using System.Text;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgXRecord
    {
        public DwgObject Parent { get; set; }
        public byte[] RawXRecordData { get; set; }

        internal override void ParseSpecific(BitReader reader, int objectBitOffsetStart, DwgVersionId version)
        {
            _dataSize = reader.Read_BL();
            RawXRecordData = reader.ReadBytes(_dataSize);
            var newReader = new BitReader(RawXRecordData);
            // TODO: actually read the XRecord
            //ReadXData(newReader);
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
            // TODO: unsure if parent handle is soft pointer (3) or hard pointer (4)
            //if (_parentHandleReference.Code != DwgHandleReferenceCode.HardPointer)
            //{
            //    throw new DwgReadException("Incorrect parent handle reference code");
            //}

            objectCache.GetObjectLazy(ResolveHandleReference(_parentHandleReference), parent => Parent = parent);
        }

        internal override void OnBeforeObjectWrite(DwgVersionId version)
        {
            _parentHandleReference = Parent is { }
                ? Parent.MakeHandleReference(DwgHandleReferenceCode.SoftPointer)
                : DwgHandleReference.Empty;
        }

        internal override void WriteSpecific(BitWriter writer, DwgVersionId version)
        {
            _dataSize = RawXRecordData.Length;
            writer.Write_BL(_dataSize);
            writer.WriteBytes(RawXRecordData);
            _objectSize = writer.BitCount;

            // handles
            writer.Write_H(_parentHandleReference);
            for (int i = 0; i < _reactorCount; i++)
            {
                writer.Write_H(_reactorHandleReferences[i]);
            }

            writer.Write_H(_xDictionaryObjectHandleReference);
        }

        internal static void ReadXData(BitReader reader)
        {
            while (reader.RemainingBytes > 0)
            {
                var indicator = reader.Read_RS();
                switch (indicator)
                {
                    case 1:
                        var length = reader.Read_RS();
                        var codePage = reader.Read_RC();
                        var sb = new StringBuilder();
                        for (int i = 0; i < length; i++)
                        {
                            var b = reader.ReadByte();
                            var c = (char)b;
                            sb.Append(c);
                        }
                        break;
                    case 10:
                        var x = reader.ReadDouble();
                        var y = reader.ReadDouble();
                        var z = reader.ReadDouble();
                        break;
                    case 40:
                    case 50:
                        var d = reader.ReadDouble();
                        break;
                    case 62:
                    case 70:
                        var shortValue = reader.ReadShort();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
