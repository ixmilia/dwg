namespace IxMilia.Dwg.Objects
{
    public partial class DwgImage
    {
        internal DwgHandleReference _imageDefHandleReference;
        internal DwgHandleReference _imageDefReactorHandleReference;

        public DwgImageDefinition ImageDefinition { get; set; } = new DwgImageDefinition();

        public DwgImageDefinitionReactor ImageDefinitionReactor { get; set; } = new DwgImageDefinitionReactor();

        internal override void ReadPostData(BitReader reader, DwgVersionId version)
        {
            _imageDefHandleReference = reader.Read_H();
            if (_imageDefHandleReference.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect image definition handle code");
            }

            _imageDefReactorHandleReference = reader.Read_H();
            if (_imageDefReactorHandleReference.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect image definition reactor handle code");
            }
        }

        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            ImageDefinition = objectCache.GetObject<DwgImageDefinition>(reader, ResolveHandleReference(_imageDefHandleReference));
            ImageDefinitionReactor = objectCache.GetObject<DwgImageDefinitionReactor>(reader, ResolveHandleReference(_imageDefReactorHandleReference));
        }

        internal override void OnBeforeEntityWrite(DwgVersionId version)
        {
            _imageDefHandleReference = ImageDefinition.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            _imageDefReactorHandleReference = ImageDefinitionReactor.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
        }

        internal override void WritePostData(BitWriter writer, DwgVersionId version)
        {
            writer.Write_H(_imageDefHandleReference);
            writer.Write_H(_imageDefReactorHandleReference);
        }
    }
}
