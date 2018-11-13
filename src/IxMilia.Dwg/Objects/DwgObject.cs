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
            PreWrite();
            SetCommonValues();
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
            obj.ValidateCommonValues();
            obj.PoseParse(reader, objectCache);
            return obj;
        }

        internal abstract void ParseSpecific(BitReader reader, DwgVersionId version);

        internal abstract void WriteSpecific(BitWriter writer, DwgObjectMap objectMap, int pointerOffset, DwgVersionId version);

        internal virtual void PreWrite()
        {
        }

        internal virtual void PoseParse(BitReader reader, DwgObjectCache objectCache)
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

            foreach (var reactorHandle in _reactorHandles)
            {
                if (reactorHandle.Code != DwgHandleReferenceCode.HardPointer)
                {
                    throw new DwgReadException("Incorrect reactor handle code.");
                }
            }

            if (!_nullHandle.IsEmpty && _nullHandle.Code != ExpectedNullHandleCode)
            {
                throw new DwgReadException("Invalid null handle code.");
            }

            if (_nullHandle.HandleOrOffset != 0)
            {
                throw new DwgReadException("Invalid null handle value.");
            }

            if (!_xDictionaryObjectHandle.IsEmpty && _xDictionaryObjectHandle.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Invalid XDictionary object handle code.");
            }

            if (_entityCount != _entityHandles.Count)
            {
                throw new DwgReadException("Mismatch between reported entity count and number of read handles.");
            }
        }
    }
}
