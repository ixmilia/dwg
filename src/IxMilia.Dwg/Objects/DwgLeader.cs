using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgLeader
    {
        internal DwgHandleReference _associatedAnnotationHandleReference;
        internal DwgHandleReference _dimStyleHandleReference;

        public DwgObject Annotation { get; set; }

        public DwgDimStyle DimensionStyle { get; set; }

        internal override IEnumerable<DwgObject> ChildItems
        {
            get
            {
                yield return Annotation;
                yield return DimensionStyle;
            }
        }

        internal override void ReadPostData(BitReader reader, DwgVersionId version)
        {
            if (version == DwgVersionId.R14)
            {
                _associatedAnnotationHandleReference = reader.Read_H();
                if (_associatedAnnotationHandleReference.Code != DwgHandleReferenceCode.SoftOwner)
                {
                    throw new DwgReadException("Incorrect associated annotation handle code");
                }
            }

            _dimStyleHandleReference = reader.Read_H();
            if (_dimStyleHandleReference.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect dimension style handle code");
            }
        }

        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            if (version == DwgVersionId.R14)
            {
                Annotation = objectCache.GetObject(reader, ResolveHandleReference(_associatedAnnotationHandleReference));
            }

            DimensionStyle = objectCache.GetObject<DwgDimStyle>(reader, ResolveHandleReference(_dimStyleHandleReference));
        }

        internal override void OnBeforeEntityWrite(DwgVersionId version)
        {
            if (version == DwgVersionId.R14)
            {
                _associatedAnnotationHandleReference = Annotation.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            }

            _dimStyleHandleReference = DimensionStyle.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
        }

        internal override void WritePostData(BitWriter writer, DwgVersionId version)
        {
            if (version == DwgVersionId.R14)
            {
                writer.Write_H(_associatedAnnotationHandleReference);
            }

            writer.Write_H(_dimStyleHandleReference);
        }
    }
}
