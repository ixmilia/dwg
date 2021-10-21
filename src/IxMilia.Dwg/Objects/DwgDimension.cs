namespace IxMilia.Dwg.Objects
{
    public abstract class DwgDimension : DwgEntity
    {
        internal DwgHandleReference _dimStyleHandleReference;
        internal DwgHandleReference _anonymousBlockHandleReference;

        public DwgDimStyle DimensionStyle { get; set; }

        public DwgBlock AnonymousBlock { get; set; }

        internal override void ReadPostData(BitReader reader, DwgVersionId version)
        {
            _dimStyleHandleReference = reader.Read_H();
            if (_dimStyleHandleReference.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect dimension style handle code");
            }

            _anonymousBlockHandleReference = reader.Read_H();
            if (_anonymousBlockHandleReference.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect anonymous block handle code");
            }
        }

        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            DimensionStyle = objectCache.GetObject<DwgDimStyle>(reader, ResolveHandleReference(_dimStyleHandleReference));
            AnonymousBlock = objectCache.GetObject<DwgBlock>(reader, ResolveHandleReference(_anonymousBlockHandleReference));
        }

        internal override void OnBeforeEntityWrite(DwgVersionId version)
        {
            _dimStyleHandleReference = DimensionStyle.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            _anonymousBlockHandleReference = AnonymousBlock.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
        }

        internal override void WritePostData(BitWriter writer, DwgVersionId version)
        {
            writer.Write_H(_dimStyleHandleReference);
            writer.Write_H(_anonymousBlockHandleReference);
        }
    }
}
