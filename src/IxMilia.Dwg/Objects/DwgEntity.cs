using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public abstract class DwgEntity : DwgObject
    {
        internal override bool IsEntity => true;

        protected bool _isGraphicPresent;
        protected byte[] _graphicsData = new byte[0];
        internal int _entityMode = 2;
        protected bool _isLineTypeByLayer;
        protected DwgHandleReference _subentityRef { get; set; }
        protected bool _noLinks;
        public DwgColor Color { get; set; }
        public double LineTypeScale { get; set; }
        protected short _invisibility;
        public DwgLayer Layer { get; set; }
        internal DwgHandleReference LayerHandle { get; set; }
        public DwgLineType LineType { get; set; }
        internal DwgHandleReference LineTypeHandle { get; set; }
        internal DwgHandleReference PreviousEntityHandle { get; set; } = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, 0);
        internal DwgHandleReference NextEntityHandle { get; set; } = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, 0);

        internal virtual void OnBeforeEntityWrite()
        {
        }

        internal virtual void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache)
        {
        }

        internal override void ReadCommonDataStart(BitReader reader)
        {
            Handle = reader.Read_H();
            _xdataMap = DwgXData.Parse(reader);
            _isGraphicPresent = reader.Read_B();
            if (_isGraphicPresent)
            {
                var graphicsSize = reader.Read_RL();
                _graphicsData = reader.Read_Bytes(graphicsSize);
            }

            _objectSize = reader.Read_RL();
            _entityMode = reader.Read_BB();
            _reactorCount = reader.Read_BL();
            _isLineTypeByLayer = reader.Read_B();
            _noLinks = reader.Read_B();
            Color = DwgColor.FromRawValue(reader.Read_BS());
            LineTypeScale = reader.Read_BD();
            _invisibility = reader.Read_BS();
        }

        internal override void ReadCommonDataEnd(BitReader reader)
        {
            if (_entityMode == 0)
            {
                _subentityRef = reader.Read_H();
            }

            for (int i = 0; i < _reactorCount; i++)
            {
                _reactorHandles.Add(reader.Read_H());
            }

            _xDictionaryObjectHandle = reader.Read_H();
            LayerHandle = reader.Read_H();
            if (!_isLineTypeByLayer)
            {
                LineTypeHandle = reader.Read_H();
            }

            if (!_noLinks)
            {
                PreviousEntityHandle = reader.Read_H();
            }

            if (!_noLinks)
            {
                NextEntityHandle = reader.Read_H();
            }
        }

        internal override int WriteCommonDataStart(BitWriter writer, IDictionary<string, int> appIdMap)
        {
            writer.Write_H(Handle);
            XData.Write(writer, appIdMap);
            writer.Write_B(_isGraphicPresent);
            if (_isGraphicPresent)
            {
                writer.Write_RL(_graphicsData.Length);
                writer.Write_Bytes(_graphicsData);
            }

            var objectSizeOffset = writer.BitCount;
            writer.Write_RL(_objectSize);
            writer.Write_BB(_entityMode);
            writer.Write_BL(_reactorCount);
            writer.Write_B(_isLineTypeByLayer);
            writer.Write_B(_noLinks);
            writer.Write_BS(Color.RawValue);
            writer.Write_BD(LineTypeScale);
            writer.Write_BS(_invisibility);
            return objectSizeOffset;
        }

        internal override void WriteCommonDataEnd(BitWriter writer)
        {
            if (_entityMode == 0)
            {
                writer.Write_H(_subentityRef);
            }

            for (int i = 0; i < _reactorCount; i++)
            {
                writer.Write_H(_reactorHandles[i]);
            }

            writer.Write_H(_xDictionaryObjectHandle);
            writer.Write_H(LayerHandle);
            if (!_isLineTypeByLayer)
            {
                writer.Write_H(LineTypeHandle);
            }

            if (!_noLinks)
            {
                writer.Write_H(PreviousEntityHandle);
            }

            if (!_noLinks)
            {
                writer.Write_H(NextEntityHandle);
            }
        }

        internal override void OnBeforeObjectWrite()
        {
            base.OnBeforeObjectWrite();
            LayerHandle = GetHandleToObject(Layer, DwgHandleReferenceCode.SoftOwner, throwOnNull: true);
            LineTypeHandle = GetHandleToObject(LineType, DwgHandleReferenceCode.SoftOwner);
            _isLineTypeByLayer = LineType == null;
            _noLinks = PreviousEntityHandle.IsValidNavigationHandle && NextEntityHandle.IsValidNavigationHandle;
            OnBeforeEntityWrite();
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache)
        {
            if (LayerHandle.Code != DwgHandleReferenceCode.SoftOwner && LayerHandle.Code != DwgHandleReferenceCode.HardOwner)
            {
                throw new DwgReadException("Incorrect layer handle code.");
            }

            if (!_isLineTypeByLayer && (LineTypeHandle.IsEmpty || !LineTypeHandle.IsValidNavigationHandle))
            {
                throw new DwgReadException("Incorrect line type handle code.");
            }

            if (!_noLinks && !PreviousEntityHandle.IsNullNavigationHandle && !PreviousEntityHandle.IsValidNavigationHandle)
            {
                throw new DwgReadException("Invalid previous entity handle code.");
            }

            if (!_noLinks && !NextEntityHandle.IsNullNavigationHandle && !NextEntityHandle.IsValidNavigationHandle)
            {
                throw new DwgReadException("Invalid next entity handle code.");
            }

            if (_entityMode == 0 && !_subentityRef.IsValidNavigationHandle)
            {
                throw new DwgReadException("Incorrect sub entity handle code.");
            }

            Layer = objectCache.GetObject<DwgLayer>(reader, GetNavigationHandle(LayerHandle).HandleOrOffset);
            if (!_isLineTypeByLayer)
            {
                LineType = objectCache.GetObject<DwgLineType>(reader, GetNavigationHandle(LineTypeHandle).HandleOrOffset);
            }

            OnAfterEntityRead(reader, objectCache);
        }
    }
}
