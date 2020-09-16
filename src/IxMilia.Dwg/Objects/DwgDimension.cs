namespace IxMilia.Dwg.Objects
{
    public abstract class DwgDimension : DwgEntity
    {
        private DwgHandleReference _dimStyleHandle;
        private DwgHandleReference _anonymousBlockHandle;

        public DwgDimStyle DimensionStyle { get; set; }

        public DwgBlock AnonymousBlock { get; set; }

        internal override void ReadPostData(BitReader reader)
        {
            _dimStyleHandle = reader.Read_H();
            if (_dimStyleHandle.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect dimension style handle code");
            }

            _anonymousBlockHandle = reader.Read_H();
            if (_anonymousBlockHandle.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect anonymous block handle code");
            }
        }

        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache)
        {
            DimensionStyle = objectCache.GetObject<DwgDimStyle>(reader, _dimStyleHandle.HandleOrOffset);
            AnonymousBlock = objectCache.GetObject<DwgBlock>(reader, _anonymousBlockHandle.HandleOrOffset);
        }

        internal override void OnBeforeEntityWrite()
        {
            _dimStyleHandle = DimensionStyle.Handle;
            _anonymousBlockHandle = AnonymousBlock.Handle;
        }

        internal override void WritePostData(BitWriter writer)
        {
            writer.Write_H(new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, _dimStyleHandle.HandleOrOffset));
            writer.Write_H(new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, _anonymousBlockHandle.HandleOrOffset));
        }
    }
}
