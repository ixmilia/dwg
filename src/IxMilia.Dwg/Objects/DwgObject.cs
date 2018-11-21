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
        protected byte[] _xData = new byte[0];
        protected int _objectSize;
        protected int _reactorCount;
        protected List<DwgHandleReference> _reactorHandles = new List<DwgHandleReference>();
        protected short _entityCount;
        protected List<DwgHandleReference> _entityHandles = new List<DwgHandleReference>();
        protected DwgHandleReference _nullHandle;
        protected DwgHandleReference _xDictionaryObjectHandle;

        internal virtual bool IsEntity => false;
        internal virtual IEnumerable<DwgObject> ChildItems => new DwgObject[0];
        internal virtual DwgHandleReferenceCode ExpectedNullHandleCode => DwgHandleReferenceCode.HardPointer;

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

            PrepareCommonValues();
            OnBeforeObjectWrite();
            SetCommonValues();
            objectMap.SetOffset(Handle.HandleOrOffset, writer.Position);

            // write object to memory so the size can be computed
            using (var ms = new MemoryStream())
            {
                var tempWriter = new BitWriter(ms);
                tempWriter.Write_BS((short)Type);
                WriteCommonDataStart(tempWriter, objectMap, pointerOffset);
                WriteSpecific(tempWriter, objectMap, pointerOffset, version);
                WriteCommonDataEnd(tempWriter, objectMap, pointerOffset);
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
            obj.ReadCommonDataStart(reader);
            obj.ParseSpecific(reader, version);
            obj.ReadCommonDataEnd(reader);

            // ensure there's no extra data
            reader.AlignToByte();
            reader.SkipBytes(Math.Max(0, crcStart - reader.Offset));

            reader.ValidateCrc(initialValue: DwgHeaderVariables.InitialCrcValue);
            obj.ValidateCommonValues();
            obj.OnAfterObjectRead(reader, objectCache);
            return obj;
        }

        internal abstract void ParseSpecific(BitReader reader, DwgVersionId version);

        internal abstract void WriteSpecific(BitWriter writer, DwgObjectMap objectMap, int pointerOffset, DwgVersionId version);

        internal virtual void ReadCommonDataStart(BitReader reader)
        {
        }

        internal virtual void ReadCommonDataEnd(BitReader reader)
        {
        }

        internal virtual void WriteCommonDataStart(BitWriter writer, DwgObjectMap objectMap, int pointerOffset)
        {
        }

        internal virtual void WriteCommonDataEnd(BitWriter writer, DwgObjectMap objectMap, int pointerOffset)
        {
        }

        internal virtual void OnBeforeObjectWrite()
        {
        }

        internal virtual void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache)
        {
        }

        private void PrepareCommonValues()
        {
            _entityHandles.Clear();
        }

        private void SetCommonValues()
        {
            _entityCount = (short)_entityHandles.Count;
        }

        private void ValidateCommonValues()
        {
            if (Handle.Code != DwgHandleReferenceCode.Declaration)
            {
                throw new DwgReadException("Invalid object handle code.");
            }

            if (!_nullHandle.IsEmpty && _nullHandle.Code != ExpectedNullHandleCode)
            {
                throw new DwgReadException("Invalid null handle code.");
            }

            if (_nullHandle.HandleOrOffset != 0)
            {
                throw new DwgReadException("Invalid null handle value.");
            }

            if (_entityCount != _entityHandles.Count)
            {
                throw new DwgReadException("Mismatch between reported entity count and number of read handles.");
            }
        }

        internal DwgHandleReference GetNavigationHandle(DwgHandleReference destinationHandle)
        {
            if (destinationHandle.PointsToNull)
            {
                return destinationHandle;
            }

            var handleDistance = destinationHandle.HandleOrOffset - Handle.HandleOrOffset;
            switch (handleDistance)
            {
                case 1:
                    return new DwgHandleReference(DwgHandleReferenceCode.HandlePlus1, 0);
                case -1:
                    return new DwgHandleReference(DwgHandleReferenceCode.HandleMinus1, 0);
                default:
                    return destinationHandle;
            }
        }

        internal DwgHandleReference GetHandleToObject(DwgObject other, DwgHandleReferenceCode absoluteCodeType)
        {
            return GetNavigationHandle(new DwgHandleReference(absoluteCodeType, other?.Handle.HandleOrOffset ?? 0));
        }
    }
}
