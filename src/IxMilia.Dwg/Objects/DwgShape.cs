using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgShape
    {
        public DwgStyle ShapeFile { get; set; }

        private DwgHandleReference _shapeFileHandleReference;

        internal override IEnumerable<DwgObject> ChildItems
        {
            get
            {
                yield return ShapeFile;
            }
        }

        internal override void ReadPostData(BitReader reader)
        {
            _shapeFileHandleReference = reader.Read_H();
            if (_shapeFileHandleReference.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect shape file handle code.");
            }
        }

        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache)
        {
            ShapeFile = objectCache.GetObject<DwgStyle>(reader, ResolveHandleReference(_shapeFileHandleReference));
        }

        internal override void OnBeforeEntityWrite()
        {
            _shapeFileHandleReference = ShapeFile.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
        }

        internal override void WritePostData(BitWriter writer)
        {
            writer.Write_H(_shapeFileHandleReference);
        }
    }
}
