using System;
using System.Collections.Generic;
using System.IO;

namespace IxMilia.Dwg.Objects
{
    public abstract partial class DwgObject
    {
        public DwgHandleReference Handle { get; internal set; }
        public abstract bool IsEntity { get; }
        public abstract DwgObjectType Type { get; }

        internal abstract IEnumerable<DwgObject> ChildItems { get; }

        internal void ClearHandles()
        {
            if (Handle.IsEmpty)
            {
                return;
            }

            Handle = default(DwgHandleReference);
            foreach (var child in ChildItems)
            {
                child.ClearHandles();
            }
        }

        internal void AssignHandles(DwgObjectMap objectMap)
        {
            objectMap.AssignHandle(this);
            foreach (var child in ChildItems)
            {
                child.AssignHandles(objectMap);
            }
        }

        internal void Write(BitWriter writer, DwgObjectMap objectMap, HashSet<int> writtenHandles, int pointerOffset)
        {
            if (!writtenHandles.Add(Handle.HandleOrOffset))
            {
                // already been written
                return;
            }

            PreWrite();
            objectMap.SetOffset(Handle.HandleOrOffset, writer.Position);

            // write object to memory so the size can be computed
            using (var ms = new MemoryStream())
            {
                var tempWriter = new BitWriter(ms);
                tempWriter.Write_BS((short)Type);
                tempWriter.Write_H(Handle);
                var xData = new byte[0]; // TODO: promote this to the object
                tempWriter.Write_BS((short)xData.Length);
                tempWriter.WriteBytes(xData);
                if (IsEntity)
                {
                    var graphicData = new byte[0]; // TODO: promote this to the object
                    tempWriter.Write_RL(graphicData.Length);
                    tempWriter.WriteBytes(graphicData);
                }

                // write object data to memory so the size can be computed
                using (var ms2 = new MemoryStream())
                {
                    var objectWriter = new BitWriter(ms2);
                    WriteSpecific(objectWriter, objectMap, pointerOffset);
                    var objectBytes = objectWriter.AsBytes();

                    tempWriter.Write_RL(objectBytes.Length * 8); // object size in bits.  not necessarily a multiple of 8?
                    tempWriter.WriteBytes(objectBytes);
                }

                var tempBytes = tempWriter.AsBytes();

                // now output everything
                writer.StartCrcCalculation(initialValue: DwgHeaderVariables.InitialCrcValue);
                writer.Write_MS(tempBytes.Length);
                writer.WriteBytes(tempBytes);
                writer.WriteCrc();
            }

            foreach (var child in ChildItems)
            {
                child.Write(writer, objectMap, writtenHandles, pointerOffset);
            }
        }

        internal static DwgObject Parse(BitReader reader, DwgObjectMap objectMap)
        {
            reader.StartCrcCheck();
            var size = reader.Read_MS();
            var crcStart = reader.Offset + size;
            var typeCode = reader.Read_BS();
            if (!Enum.IsDefined(typeof(DwgObjectType), typeCode))
            {
                // unsupported
                return null;
            }

            var type = (DwgObjectType)typeCode;
            var obj = CreateObject(type);
            obj.ParseData(reader);

            // ensure there's no extra data
            reader.AlignToByte();
            reader.SkipBytes(Math.Max(0, crcStart - reader.Offset));

            reader.ValidateCrc(initialValue: DwgHeaderVariables.InitialCrcValue);
            obj.PoseParse(reader, objectMap);
            return obj;
        }

        internal static T ParseSpecific<T>(BitReader reader, DwgObjectMap objectMap) where T: DwgObject
        {
            var obj = Parse(reader, objectMap);
            if (obj == null)
            {
                throw new DwgReadException("Unsupported object cannot be read");
            }

            if (obj is T converted)
            {
                return converted;
            }

            throw new DwgReadException($"Expected to parse {typeof(T)} but got {obj.GetType().Name}");
        }

        private void ParseData(BitReader reader)
        {
            Handle = reader.Read_H();
            var xDataSize = reader.Read_BS();
            var xData = reader.ReadBytes(xDataSize);
            if (IsEntity)
            {
                var hasGraphic = reader.Read_B();
                if (hasGraphic)
                {
                    var graphicSize = reader.Read_RL();
                    var graphicData = reader.ReadBytes(graphicSize);
                }
            }

            var dataSizeInBits = reader.Read_RL();
            if (IsEntity)
            {
                var flags = reader.ReadBytes(6);
                var commonParams = reader.ReadBytes(6);
            }

            ParseSpecific(reader);
        }

        internal abstract void ParseSpecific(BitReader reader);

        internal abstract void WriteSpecific(BitWriter writer, DwgObjectMap objectMap, int pointerOffset);

        internal virtual void PoseParse(BitReader reader, DwgObjectMap objectMap)
        {
        }

        internal virtual void PreWrite()
        {
        }
    }
}
