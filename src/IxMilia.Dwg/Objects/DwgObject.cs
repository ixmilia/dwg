using System;
using System.Collections.Generic;
using System.IO;

namespace IxMilia.Dwg.Objects
{
    public abstract partial class DwgObject
    {
        public abstract DwgObjectType Type { get; }
        public DwgHandleReference Handle { get; internal set; }
        protected int _xDataSize;
        protected byte[] _xData;
        protected int _objectSize;
        protected int _reactorCount;

        internal virtual bool IsEntity => false;
        internal virtual IEnumerable<DwgObject> ChildItems => new DwgObject[0];

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

        internal void Write(BitWriter writer, DwgObjectMap objectMap, HashSet<int> writtenHandles, int pointerOffset, DwgVersionId version)
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
                WriteSpecific(tempWriter, objectMap, pointerOffset, version);
                var tempBytes = tempWriter.AsBytes();

                // now output everything
                writer.StartCrcCalculation(initialValue: DwgHeaderVariables.InitialCrcValue);
                writer.Write_MS(tempBytes.Length);
                writer.WriteBytes(tempBytes);
                writer.WriteCrc();
            }

            foreach (var child in ChildItems)
            {
                child.Write(writer, objectMap, writtenHandles, pointerOffset, version);
            }
        }

        internal static DwgObject Parse(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
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
            obj.ParseSpecific(reader, version);

            // ensure there's no extra data
            reader.AlignToByte();
            reader.SkipBytes(Math.Max(0, crcStart - reader.Offset));

            reader.ValidateCrc(initialValue: DwgHeaderVariables.InitialCrcValue);
            obj.PoseParse(reader, objectCache);
            return obj;
        }

        internal abstract void ParseSpecific(BitReader reader, DwgVersionId version);

        internal abstract void WriteSpecific(BitWriter writer, DwgObjectMap objectMap, int pointerOffset, DwgVersionId version);

        internal virtual void PoseParse(BitReader reader, DwgObjectCache objectCache)
        {
            if (Handle.Code != DwgHandleReferenceCode.Declaration)
            {
                throw new DwgReadException("Invalid object handle code.");
            }
        }

        internal virtual void PreWrite()
        {
        }
    }
}
