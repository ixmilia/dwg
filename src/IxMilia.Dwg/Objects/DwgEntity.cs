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

        /// <summary>
        /// No previous/next entity links; assume previous is (Handle - 1) and next is (Handle + 1)
        /// </summary>
        protected bool _noLinks;
        public DwgColor Color { get; set; }
        public double LineTypeScale { get; set; } = 1.0;
        protected short _invisibility;
        public DwgLayer Layer { get; set; }
        internal DwgHandleReference LayerHandleReference { get; set; }
        public DwgLineType? LineType { get; set; }
        internal DwgHandleReference LineTypeHandleReference { get; set; }
        internal DwgHandleReference PreviousEntityHandle { get; set; } = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, 0);
        internal DwgHandleReference NextEntityHandle { get; set; } = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, 0);

        // only exists for object creation during parsing
        internal DwgEntity()
        {
            Layer = null!;
        }

        internal virtual void OnBeforeEntityWrite(DwgVersionId version)
        {
        }

        internal virtual void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
        }

        internal override void ReadCommonDataStart(BitReader reader)
        {
            Handle = reader.Read_H().AsDeclarationHandle();
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
                _reactorHandleReferences.Add(reader.Read_H());
            }

            _xDictionaryObjectHandleReference = reader.Read_H();
            LayerHandleReference = reader.Read_H();
            if (!_isLineTypeByLayer)
            {
                LineTypeHandleReference = reader.Read_H();
            }

            if (_noLinks)
            {
                PreviousEntityHandle = new DwgHandleReference(DwgHandleReferenceCode.HandleMinus1, 0);
                NextEntityHandle = new DwgHandleReference(DwgHandleReferenceCode.HandlePlus1, 0);
            }
            else
            {
                PreviousEntityHandle = reader.Read_H();
                NextEntityHandle = reader.Read_H();
            }
        }

        internal override int WriteCommonDataStart(BitWriter writer, IDictionary<string, DwgHandle> appIdMap)
        {
            writer.Write_H(Handle.MakeDeclarationHandleReference());
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
                writer.Write_H(_reactorHandleReferences[i]);
            }

            writer.Write_H(_xDictionaryObjectHandleReference);
            writer.Write_H(LayerHandleReference);
            if (!_isLineTypeByLayer)
            {
                writer.Write_H(LineTypeHandleReference);
            }

            if (!_noLinks)
            {
                writer.Write_H(PreviousEntityHandle);
                writer.Write_H(NextEntityHandle);
            }
        }

        internal override void OnBeforeObjectWrite(DwgVersionId version)
        {
            base.OnBeforeObjectWrite(version);
            LayerHandleReference = GetHandleToObject(Layer, DwgHandleReferenceCode.SoftOwner, throwOnNull: true);
            LineTypeHandleReference = GetHandleToObject(LineType, DwgHandleReferenceCode.SoftOwner);
            _isLineTypeByLayer = LineType == null;

            _noLinks = false; // assume previous/next links, unless we decide otherwise
            var previousHandle = ResolveHandleReference(PreviousEntityHandle);
            var nextHandle = ResolveHandleReference(NextEntityHandle);
            if (!PreviousEntityHandle.PointsToNull &&
                !NextEntityHandle.PointsToNull &&
                (uint)previousHandle == (uint)Handle - 1 &&
                (uint)nextHandle == (uint)Handle + 1)
            {
                // if both previous and next handles exist and they point to -1/+1 respectively then omit writing them
                _noLinks = true;
            }

            OnBeforeEntityWrite(version);
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            if (LayerHandleReference.Code != DwgHandleReferenceCode.SoftOwner && LayerHandleReference.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException($"Incorrect layer handle code {LayerHandleReference.Code}.");
            }

            if (!_isLineTypeByLayer && (LineTypeHandleReference.Code != DwgHandleReferenceCode.SoftOwner))
            {
                throw new DwgReadException($"Incorrect line type handle code {LineTypeHandleReference.Code}.");
            }

            if (!_noLinks && !PreviousEntityHandle.IsNullNavigationHandle && !PreviousEntityHandle.IsValidNavigationHandle)
            {
                throw new DwgReadException($"Invalid previous entity handle code {PreviousEntityHandle.Code}.");
            }

            if (!_noLinks && !NextEntityHandle.IsNullNavigationHandle && !NextEntityHandle.IsValidNavigationHandle)
            {
                throw new DwgReadException($"Invalid next entity handle code {NextEntityHandle.Code}.");
            }

            if (_entityMode == 0 && !_subentityRef.IsValidNavigationHandle)
            {
                throw new DwgReadException($"Incorrect sub entity handle code {_subentityRef.Code}.");
            }

            Layer = objectCache.GetObject<DwgLayer>(reader, ResolveHandleReference(LayerHandleReference));
            if (!_isLineTypeByLayer)
            {
                LineType = objectCache.GetObject<DwgLineType>(reader, ResolveHandleReference(LineTypeHandleReference));
            }

            OnAfterEntityRead(reader, objectCache, version);
        }

        internal void AssignSubentityReference(DwgHandle ownerHandle)
        {
            if (_entityMode == 0)
            {
                _subentityRef = ownerHandle.MakeHandleReference(DwgHandleReferenceCode.HardPointer);
            }
        }
    }
}
