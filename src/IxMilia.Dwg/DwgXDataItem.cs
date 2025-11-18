using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace IxMilia.Dwg
{
    public abstract class DwgXDataItem
    {
        public abstract byte Code { get; }

        internal abstract void WriteData(BitWriter writer);

        internal void Write(BitWriter writer)
        {
            writer.WriteByte(Code);
            WriteData(writer);
        }

        internal static DwgXDataItem? Parse(BitReader reader, bool readListClosureAsNull = false)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case 0:
                    return DwgXDataString.ParseString(reader);
                case 2:
                    return DwgXDataItemList.ParseItemList(reader, readListClosureAsNull);
                case 3:
                    return DwgXDataLayerReference.ParseLayerReference(reader);
                case 4:
                    return DwgXDataBinaryChunk.ParseBinary(reader);
                case 5:
                    return DwgXDataEntityReference.ParseEntityReference(reader);
                case 10:
                    return DwgXDataRealTriple.ParseTriple(reader);
                case 11:
                    return DwgXDataWorldSpacePosition.ParsePosition(reader);
                case 12:
                    return DwgXDataWorldSpaceDisplacement.ParseDisplacement(reader);
                case 13:
                    return DwgXDataWorldDirection.ParseDirection(reader);
                case 40:
                    return DwgXDataReal.ParseReal(reader);
                case 41:
                    return DwgXDataDistance.ParseDistance(reader);
                case 42:
                    return DwgXDataScaleFactor.ParseScaleFactor(reader);
                case 70:
                    return DwgXDataShort.ParseShort(reader);
                case 71:
                    return DwgXDataLong.ParseLong(reader);
                default:
                    throw new DwgReadException($"Unexpected/unsupported XData code {type}.");
            }
        }

        internal static IList<DwgXDataItem> ParseItems(BitReader reader)
        {
            var items = new List<DwgXDataItem>();
            while (reader.RemainingBytes > 0)
            {
                var item = Parse(reader);
                if (item is not null)
                {
                    items.Add(item);
                }
            }

            return items;
        }
    }

    public class DwgXDataString : DwgXDataItem
    {
        public override byte Code => 0;

        public short CodePage { get; }
        public string Value { get; }

        internal DwgXDataString(string value)
            : this(30, value)
        {
        }

        public DwgXDataString(short codePage, string value)
        {
            CodePage = codePage;
            Value = value;
        }

        internal override void WriteData(BitWriter writer)
        {
            writer.WriteByte((byte)Value.Length);
            writer.WriteShortBigEndian(CodePage);
            var data = Encoding.UTF8.GetBytes(Value); // TODO: honor code page
            writer.WriteBytes(data);
        }

        internal static DwgXDataString ParseString(BitReader reader)
        {
            var length = (int)reader.ReadByte();
            var codePage = reader.ReadShortBigEndian();
            // TODO: code page 12 and 30 correspond to ansi 1252
            var bytes = reader.ReadBytes(length);
            // TODO: honor code page
            var value = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            return new DwgXDataString(codePage, value);
        }
    }

    public class DwgXDataItemList : DwgXDataItem, IList<DwgXDataItem>
    {
        public override byte Code => 2;

        private IList<DwgXDataItem> _items;

        public DwgXDataItemList()
        {
            _items = new List<DwgXDataItem>();
        }

        internal override void WriteData(BitWriter writer)
        {
            writer.WriteByte(0);
            foreach (var item in _items)
            {
                item.Write(writer);
            }

            writer.WriteByte(2);
            writer.WriteByte(1);
        }

        internal static DwgXDataItemList? ParseItemList(BitReader reader, bool readListClosureAsNull = false)
        {
            var openingMark = reader.ReadByte();
            if (readListClosureAsNull && openingMark == 1)
            {
                return null;
            }

            if (openingMark != 0)
            {
                throw new DwgReadException($"Expected opening marker of 0x00 but found 0x{openingMark:X2}.");
            }

            var list = new DwgXDataItemList();
            var item = Parse(reader, readListClosureAsNull: true);
            while (item != null)
            {
                list.Add(item);
                item = Parse(reader, readListClosureAsNull: true);
            }

            return list;
        }

        #region interface implementation

        public int Count => _items.Count;

        public bool IsReadOnly => _items.IsReadOnly;

        public DwgXDataItem this[int index] { get => _items[index]; set => _items[index] = value; }

        public int IndexOf(DwgXDataItem item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, DwgXDataItem item)
        {
            _items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        public void Add(DwgXDataItem item)
        {
            _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(DwgXDataItem item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(DwgXDataItem[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public bool Remove(DwgXDataItem item)
        {
            return _items.Remove(item);
        }

        public IEnumerator<DwgXDataItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }

        #endregion
    }

    public class DwgXDataLayerReference : DwgXDataItem
    {
        public override byte Code => 3;

        public int Handle { get; }

        public DwgXDataLayerReference(int handle)
        {
            Handle = handle;
        }

        internal override void WriteData(BitWriter writer)
        {
            var hexString = Handle.ToString("X8");
            var hexData = Encoding.UTF8.GetBytes(hexString);
            writer.WriteBytes(hexData);
        }

        internal static DwgXDataLayerReference ParseLayerReference(BitReader reader)
        {
            var hexData = reader.ReadBytes(8);
            var hexString = Encoding.UTF8.GetString(hexData, 0, hexData.Length);
            if (!int.TryParse(hexString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var handle))
            {
                throw new DwgReadException("Unable to read XData layer handle.");
            }

            return new DwgXDataLayerReference(handle);
        }
    }

    public class DwgXDataBinaryChunk : DwgXDataItem
    {
        public override byte Code => 4;

        public byte[] Data { get; }

        public DwgXDataBinaryChunk(byte[] data)
        {
            Data = data;
            if (Data.Length > 256)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "Data must not exceed 256 bytes.");
            }
        }

        internal override void WriteData(BitWriter writer)
        {
            writer.WriteByte((byte)Data.Length);
            writer.WriteBytes(Data);
        }

        internal static DwgXDataBinaryChunk ParseBinary(BitReader reader)
        {
            var count = (int)reader.ReadByte();
            var data = reader.ReadBytes(count);
            return new DwgXDataBinaryChunk(data);
        }
    }

    public class DwgXDataEntityReference : DwgXDataItem
    {
        public override byte Code => 5;

        public int Handle { get; }

        public DwgXDataEntityReference(int handle)
        {
            Handle = handle;
        }

        internal override void WriteData(BitWriter writer)
        {
            var hexString = Handle.ToString("X8");
            var hexData = Encoding.UTF8.GetBytes(hexString);
            writer.WriteBytes(hexData);
        }

        internal static DwgXDataEntityReference ParseEntityReference(BitReader reader)
        {
            var hexData = reader.ReadBytes(8);
            var hexString = Encoding.UTF8.GetString(hexData, 0, hexData.Length);
            if (!int.TryParse(hexString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var handle))
            {
                throw new DwgReadException("Unable to read XData layer handle.");
            }

            return new DwgXDataEntityReference(handle);
        }
    }

    public class DwgXDataRealTriple : DwgXDataItem
    {
        public override byte Code => 10;

        public DwgPoint Value { get; }

        public DwgXDataRealTriple(DwgPoint value)
        {
            Value = value;
        }

        internal override void WriteData(BitWriter writer)
        {
            writer.WriteDouble(Value.X);
            writer.WriteDouble(Value.Y);
            writer.WriteDouble(Value.Z);
        }

        internal static DwgXDataRealTriple ParseTriple(BitReader reader)
        {
            var x = reader.ReadDouble();
            var y = reader.ReadDouble();
            var z = reader.ReadDouble();
            return new DwgXDataRealTriple(new DwgPoint(x, y, z));
        }
    }

    public class DwgXDataWorldSpacePosition: DwgXDataItem
    {
        public override byte Code => 11;

        public DwgPoint Value { get; }

        public DwgXDataWorldSpacePosition(DwgPoint value)
        {
            Value = value;
        }

        internal override void WriteData(BitWriter writer)
        {
            writer.WriteDouble(Value.X);
            writer.WriteDouble(Value.Y);
            writer.WriteDouble(Value.Z);
        }

        internal static DwgXDataWorldSpacePosition ParsePosition(BitReader reader)
        {
            var x = reader.ReadDouble();
            var y = reader.ReadDouble();
            var z = reader.ReadDouble();
            return new DwgXDataWorldSpacePosition(new DwgPoint(x, y, z));
        }
    }

    public class DwgXDataWorldSpaceDisplacement : DwgXDataItem
    {
        public override byte Code => 12;

        public DwgPoint Value { get; }

        public DwgXDataWorldSpaceDisplacement(DwgPoint value)
        {
            Value = value;
        }

        internal override void WriteData(BitWriter writer)
        {
            writer.WriteDouble(Value.X);
            writer.WriteDouble(Value.Y);
            writer.WriteDouble(Value.Z);
        }

        internal static DwgXDataWorldSpaceDisplacement ParseDisplacement(BitReader reader)
        {
            var x = reader.ReadDouble();
            var y = reader.ReadDouble();
            var z = reader.ReadDouble();
            return new DwgXDataWorldSpaceDisplacement(new DwgPoint(x, y, z));
        }
    }

    public class DwgXDataWorldDirection : DwgXDataItem
    {
        public override byte Code => 13;

        public DwgVector Value { get; }

        public DwgXDataWorldDirection(DwgVector value)
        {
            Value = value;
        }

        internal override void WriteData(BitWriter writer)
        {
            writer.WriteDouble(Value.X);
            writer.WriteDouble(Value.Y);
            writer.WriteDouble(Value.Z);
        }

        internal static DwgXDataWorldDirection ParseDirection(BitReader reader)
        {
            var x = reader.ReadDouble();
            var y = reader.ReadDouble();
            var z = reader.ReadDouble();
            return new DwgXDataWorldDirection(new DwgVector(x, y, z));
        }
    }

    public class DwgXDataReal : DwgXDataItem
    {
        public override byte Code => 40;

        public double Value { get; }

        public DwgXDataReal(double value)
        {
            Value = value;
        }

        internal override void WriteData(BitWriter writer)
        {
            writer.WriteDouble(Value);
        }

        internal static DwgXDataReal ParseReal(BitReader reader)
        {
            var value = reader.ReadDouble();
            return new DwgXDataReal(value);
        }
    }

    public class DwgXDataDistance : DwgXDataItem
    {
        public override byte Code => 41;

        public double Value { get; }

        public DwgXDataDistance(double value)
        {
            Value = value;
        }

        internal override void WriteData(BitWriter writer)
        {
            writer.WriteDouble(Value);
        }

        internal static DwgXDataDistance ParseDistance(BitReader reader)
        {
            var value = reader.ReadDouble();
            return new DwgXDataDistance(value);
        }
    }

    public class DwgXDataScaleFactor : DwgXDataItem
    {
        public override byte Code => 42;

        public double Value { get; }

        public DwgXDataScaleFactor(double value)
        {
            Value = value;
        }

        internal override void WriteData(BitWriter writer)
        {
            writer.WriteDouble(Value);
        }

        internal static DwgXDataScaleFactor ParseScaleFactor(BitReader reader)
        {
            var value = reader.ReadDouble();
            return new DwgXDataScaleFactor(value);
        }
    }

    public class DwgXDataShort : DwgXDataItem
    {
        public override byte Code => 70;

        public short Value { get; }

        internal DwgXDataShort(bool value)
            : this((short)(value ? 1 : 0))
        {
        }

        public DwgXDataShort(short value)
        {
            Value = value;
        }

        internal override void WriteData(BitWriter writer)
        {
            writer.WriteShort(Value);
        }

        internal static DwgXDataShort ParseShort(BitReader reader)
        {
            var value = reader.ReadShort();
            return new DwgXDataShort(value);
        }
    }

    public class DwgXDataLong : DwgXDataItem
    {
        public override byte Code => 71;

        public int Value { get; }

        public DwgXDataLong(int value)
        {
            Value = value;
        }

        internal override void WriteData(BitWriter writer)
        {
            writer.WriteInt(Value);
        }

        internal static DwgXDataLong ParseLong(BitReader reader)
        {
            var value = reader.ReadInt();
            return new DwgXDataLong(value);
        }
    }
}
