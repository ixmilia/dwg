namespace IxMilia.Dwg.Objects
{
    public abstract class DwgEntity : DwgObject
    {
        internal override bool IsEntity => true;

        protected bool _isGraphicPresent;
        protected int _graphicsSize;
        protected byte[] _graphicsData;
        protected int _entityMode;
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
        internal DwgHandleReference PreviousEntityHandle { get; set; }
        internal DwgHandleReference NextEntityHandle { get; set; }

        internal virtual void OnBeforeEntityWrite()
        {
        }

        internal virtual void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache)
        {
        }

        internal override void OnBeforeObjectWrite()
        {
            LayerHandle = GetHandleToObject(Layer, DwgHandleReferenceCode.SoftOwner);
            LineTypeHandle = GetHandleToObject(LineType, DwgHandleReferenceCode.SoftOwner);
            _isLineTypeByLayer = LineType == null;
            _noLinks = _subentityRef.IsEmpty;
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

            if (!PreviousEntityHandle.IsValidNavigationHandle)
            {
                throw new DwgReadException("Invalid previous entity handle code.");
            }

            if (!NextEntityHandle.IsValidNavigationHandle)
            {
                throw new DwgReadException("Invalid next entity handle code.");
            }

            if (!_noLinks && !_subentityRef.IsEmpty && !_subentityRef.IsValidNavigationHandle)
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
