using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IxMilia.Dwg.Objects
{
    public abstract partial class DwgObject
    {
        public abstract DwgObjectType Type { get; }
        public DwgHandleReference Handle { get; internal set; }
        internal DwgXData XData { get; set; } = new DwgXData();
        protected int _objectSize;
        protected int _reactorCount;
        internal List<DwgHandleReference> _reactorHandles = new List<DwgHandleReference>();
        internal short _entityCount;
        internal List<DwgHandleReference> _entityHandles = new List<DwgHandleReference>();
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

        internal virtual void AssignHandles(DwgObjectMap objectMap)
        {
            objectMap.AssignHandle(this);
            foreach (var child in ChildItems)
            {
                child.AssignHandles(objectMap);
            }
        }

        internal void Write(BitWriter writer, DwgObjectMap objectMap, HashSet<int> writtenHandles, int pointerOffset, DwgVersionId version, IDictionary<string, short> classMap)
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

            WriteCoreRaw(writer, version, classMap);

            foreach (var child in ChildItems)
            {
                child.Write(writer, objectMap, writtenHandles, pointerOffset, version, classMap);
            }
        }

        internal void WriteCoreRaw(BitWriter writer, DwgVersionId version, IDictionary<string, short> classMap)
        {
            // write object to memory so the size can be computed
            using (var ms = new MemoryStream())
            {
                var tempWriter = new BitWriter(ms);
                var typeCode = (short)Type;
                if (typeCode >= 500)
                {
                    var className = DwgObjectTypeExtensions.ClassNameFromTypeCode(Type);
                    if (classMap.TryGetValue(className, out typeCode))
                    {
                        typeCode += 500;
                    }
                    else
                    {
                        // unknown
                        return;
                    }
                }

                tempWriter.Write_BS(typeCode);
                WriteCommonDataStart(tempWriter);
                WriteSpecific(tempWriter, version);
                WriteCommonDataEnd(tempWriter);
                WritePostData(tempWriter);
                var tempBytes = tempWriter.AsBytes();

                // now output everything
                writer.StartCrcCalculation(initialValue: DwgHeaderVariables.InitialCrcValue);
                writer.Write_MS(tempBytes.Length);
                writer.WriteBytes(tempBytes);
                writer.WriteCrc();
            }
        }

        internal static DwgObject ParseRaw(BitReader reader, DwgVersionId version, IList<DwgClassDefinition> classes)
        {
            reader.StartCrcCheck();
            var size = reader.Read_MS();
            var crcStart = reader.Offset + size;
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
                    // unknown
                    return null;
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
            obj.ParseSpecific(reader, version);
            obj.ReadCommonDataEnd(reader);
            obj.ReadPostData(reader);

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
            obj?.OnAfterObjectRead(reader, objectCache);
            return obj;
        }

        internal abstract void ParseSpecific(BitReader reader, DwgVersionId version);

        internal abstract void WriteSpecific(BitWriter writer, DwgVersionId version);

        internal virtual void ReadCommonDataStart(BitReader reader)
        {
            Handle = reader.Read_H();
            XData = DwgXData.Parse(reader);
            _objectSize = reader.Read_RL();
            _reactorCount = reader.Read_BL();
        }

        internal virtual void ReadCommonDataEnd(BitReader reader)
        {
        }

        internal virtual void ReadPostData(BitReader reader)
        {
        }

        internal virtual void WriteCommonDataStart(BitWriter writer)
        {
            writer.Write_H(Handle);
            XData.Write(writer);
            writer.Write_RL(_objectSize);
            writer.Write_BL(_reactorCount);
        }

        internal virtual void WriteCommonDataEnd(BitWriter writer)
        {
        }

        internal virtual void WritePostData(BitWriter writer)
        {
        }

        internal virtual void OnBeforeObjectWrite()
        {
            _reactorCount = _reactorHandles.Count;
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

        internal DwgHandleReference GetHandleToObject(DwgObject other, DwgHandleReferenceCode absoluteCodeType, bool throwOnNull = false)
        {
            if (throwOnNull && other == null)
            {
                throw new ArgumentNullException(nameof(other), "The object handle was not allowed to be null.");
            }

            return GetNavigationHandle(new DwgHandleReference(absoluteCodeType, other?.Handle.HandleOrOffset ?? 0));
        }
    }
}
