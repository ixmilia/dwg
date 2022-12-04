using System;
using System.Collections.Generic;
using System.IO;

namespace IxMilia.Dwg.Objects
{
    public abstract partial class DwgObject
    {
        public abstract DwgObjectType Type { get; }
        public DwgHandle Handle { get; internal set; }
        public DwgXData XData
        {
            get => _xdata;
            set => _xdata = value ?? throw new ArgumentNullException("value");
        }

        internal IDictionary<DwgHandle, IList<DwgXDataItem>> _xdataMap;
        private bool _objectSizeVerified;
        protected int _objectSize;
        protected int _reactorCount;
        internal List<DwgHandleReference> _reactorHandleReferences = new List<DwgHandleReference>();
        internal short _entityCount;
        internal List<DwgHandleReference> _entityHandleReferences = new List<DwgHandleReference>();
        protected DwgHandleReference _nullHandleReference;
        protected DwgHandleReference _xDictionaryObjectHandleReference = new DwgHandleReference(DwgHandleReferenceCode.SoftPointer, 0);
        private DwgXData _xdata = new DwgXData();

        internal virtual bool IsEntity => false;
        internal virtual IEnumerable<DwgObject> ChildItems => new DwgObject[0];
        internal virtual DwgHandleReferenceCode ExpectedNullHandleCode => DwgHandleReferenceCode.HardPointer;

        internal void ClearHandles()
        {
            if (Handle.IsNull)
            {
                return;
            }

            Handle = default(DwgHandle);
            foreach (var child in ChildItems)
            {
                child.ClearHandles();
            }
        }

        internal virtual void AssignHandles(DwgObjectMap objectMap)
        {
            objectMap.AssignHandle(this);
            foreach (var child in ChildItems)
            {
                child.AssignHandles(objectMap);
            }
        }

        internal DwgHandle ResolveHandleReference(DwgHandleReference handleReference) => Handle.ResolveHandleReference(handleReference);

        internal DwgHandleReference MakeHandleReference(DwgHandleReferenceCode code) => Handle.MakeHandleReference(code);

        internal void Write(BitWriter writer, DwgObjectMap objectMap, HashSet<DwgHandle> writtenHandles, DwgVersionId version, IDictionary<string, short> classMap, IDictionary<string, DwgHandle> appIdMap)
        {
            if (!writtenHandles.Add(Handle))
            {
                // already been written
                return;
            }

            PrepareCommonValues();
            OnBeforeObjectWrite(version);
            SetCommonValues();
            objectMap.SetOffset(Handle, writer.Position);

            WriteCoreRaw(writer, version, classMap, appIdMap);

            foreach (var child in ChildItems)
            {
                child.Write(writer, objectMap, writtenHandles, version, classMap, appIdMap);
            }
        }

        internal void WriteCoreRaw(BitWriter writer, DwgVersionId version, IDictionary<string, short> classMap, IDictionary<string, DwgHandle> appIdMap)
        {
            // write object to memory so the size can be computed
            using (var ms = new MemoryStream())
            {
                var tempWriter = new BitWriter(ms);
                var typeCode = (short)Type;
                if (typeCode >= 500)
                {
                    var className = DwgObjectTypeExtensions.ClassNameFromTypeCode(Type);
                    if (!classMap.TryGetValue(className, out typeCode))
                    {
                        throw new InvalidOperationException($"Missing class '{className}' in drawing's {nameof(DwgDrawing.Classes)} collection");
                    }
                }

                _objectSize = 0;
                tempWriter.Write_BS(typeCode);
                var objectSizeOffset = WriteCommonDataStart(tempWriter, appIdMap);
                WriteSpecific(tempWriter, version);
                WriteCommonDataEnd(tempWriter);
                WritePostData(tempWriter, version);
                var tempBytes = tempWriter.AsBytes();

                if (_objectSize == 0)
                {
                    throw new InvalidOperationException($"{nameof(_objectSize)} field not set for object type {Type}.  This should never happen; it means there's an error with this library.");
                }

                BitWriter.WriteRLAtPosition(tempBytes, _objectSize, objectSizeOffset);

                // now output everything
                writer.StartCrcCalculation(initialValue: DwgHeaderVariables.InitialCrcValue);
                writer.Write_MS(tempBytes.Length);
                writer.WriteBytes(tempBytes);
                writer.WriteCrc();
            }
        }

        internal static DwgObject ParseRaw(BitReader reader, DwgVersionId version, IList<DwgClassDefinition> classes)
        {
            var objectStart = reader.Offset;
            reader.StartCrcCheck();
            var size = reader.Read_MS();
            var crcStart = reader.Offset + size;
            var objectBitOffsetStart = reader.BitOffset;
            var typeCode = reader.Read_BS();
            if (typeCode >= 500)
            {
                if ((typeCode - 500) <= classes.Count)
                {
                    // non-static type code
                    var className = classes[typeCode - 500].DxfClassName;
                    var dynamicTypeCode = DwgObjectTypeExtensions.TypeCodeFromClassName(className);
                    if (!dynamicTypeCode.HasValue)
                    {
                        // unknown dynamic object type
                        return null;
                    }

                    typeCode = dynamicTypeCode.GetValueOrDefault();
                }
                else
                {
                    throw new DwgReadException($"Unable to find class definition wtih index {typeCode - 500} for object at offset {objectStart}.");
                }
            }
            else if (!Enum.IsDefined(typeof(DwgObjectType), typeCode))
            {
                // unsupported
                return null;
            }

            var type = (DwgObjectType)typeCode;
            var obj = CreateObject(type);
            obj.ReadCommonDataStart(reader);
            obj.ParseSpecific(reader, objectBitOffsetStart, version);
            obj.ReadCommonDataEnd(reader);
            obj.ReadPostData(reader, version);

            if (!obj._objectSizeVerified)
            {
                throw new InvalidOperationException($"Size was not verified for object of type {type}.  This should never happen; it means there's an error with this library.");
            }

            // ensure there's no extra data
            reader.AlignToByte();
            reader.SkipBytes(Math.Max(0, crcStart - reader.Offset));

            reader.ValidateCrc(initialValue: DwgHeaderVariables.InitialCrcValue);
            obj.ValidateCommonValues();

            return obj;
        }

        internal static DwgObject Parse(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            var obj = ParseRaw(reader, version, objectCache.Classes);
            if (obj != null)
            {
                obj.OnAfterObjectRead(reader, objectCache, version);
                obj.XData = DwgXData.FromMap(reader, objectCache, obj._xdataMap);
                obj._xdataMap = null;
            }

            return obj;
        }

        internal abstract void ParseSpecific(BitReader reader, int objectBitOffsetStart, DwgVersionId version);

        internal abstract void WriteSpecific(BitWriter writer, DwgVersionId version);

        internal void AssertObjectSize(BitReader reader, int objectBitOffsetStart)
        {
            _objectSizeVerified = true;
            var currentOffset = reader.BitOffset;
            var actualSize = currentOffset - objectBitOffsetStart;
            if (actualSize != _objectSize)
            {
                throw new DwgReadException($"Expected object size of {_objectSize} but actually read {actualSize} for object of type {Type}.");
            }
        }

        internal virtual void ReadCommonDataStart(BitReader reader)
        {
            Handle = reader.Read_H().AsDeclarationHandle();
            _xdataMap = DwgXData.Parse(reader);
            _objectSize = reader.Read_RL();
            _reactorCount = reader.Read_BL();
        }

        internal virtual void ReadCommonDataEnd(BitReader reader)
        {
        }

        internal virtual void ReadPostData(BitReader reader, DwgVersionId version)
        {
        }

        internal virtual int WriteCommonDataStart(BitWriter writer, IDictionary<string, DwgHandle> appIdMap)
        {
            writer.Write_H(MakeHandleReference(DwgHandleReferenceCode.Declaration));
            XData.Write(writer, appIdMap);
            var objectSizeOffset = writer.BitCount;
            writer.Write_RL(_objectSize);
            writer.Write_BL(_reactorCount);
            return objectSizeOffset;
        }

        internal virtual void WriteCommonDataEnd(BitWriter writer)
        {
        }

        internal virtual void WritePostData(BitWriter writer, DwgVersionId version)
        {
        }

        internal virtual void OnBeforeObjectWrite(DwgVersionId version)
        {
        }

        internal virtual void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
        }

        internal void PrepareCommonValues()
        {
            _entityHandleReferences.Clear();
            _reactorCount = _reactorHandleReferences.Count;
        }

        private void SetCommonValues()
        {
            _entityCount = (short)_entityHandleReferences.Count;
        }

        private void ValidateCommonValues()
        {
            if (!_nullHandleReference.IsEmpty && _nullHandleReference.Code != ExpectedNullHandleCode)
            {
                throw new DwgReadException("Invalid null handle code.");
            }

            if (_nullHandleReference.HandleOrOffset != 0)
            {
                throw new DwgReadException("Invalid null handle value.");
            }

            if (_entityCount != _entityHandleReferences.Count)
            {
                throw new DwgReadException("Mismatch between reported entity count and number of read handles.");
            }
        }

        internal DwgHandleReference GetHandleToObject(DwgObject other, DwgHandleReferenceCode absoluteCodeType, bool throwOnNull = false)
        {
            if (throwOnNull && other == null)
            {
                throw new ArgumentNullException(nameof(other), "The object handle was not allowed to be null.");
            }

            return Handle.MakeNavigationReference(other?.Handle ?? default(DwgHandle), absoluteCodeType);
        }
    }
}
