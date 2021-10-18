using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgViewPortEntity
    {
        public DwgViewPortEntityHeader ViewPortEntityHeader { get; set; }

        internal DwgHandleReference _viewPortEntityHeaderHandleReference;

        internal override IEnumerable<DwgObject> ChildItems
        {
            get
            {
                yield return ViewPortEntityHeader;
            }
        }

        internal override void ReadPostData(BitReader reader)
        {
            _viewPortEntityHeaderHandleReference = reader.Read_H();
            if (_viewPortEntityHeaderHandleReference.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect view port entity header handle code.");
            }
        }

        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache)
        {
            ViewPortEntityHeader = objectCache.GetObject<DwgViewPortEntityHeader>(reader, ResolveHandleReference(_viewPortEntityHeaderHandleReference));
        }

        internal override void OnBeforeEntityWrite()
        {
            _viewPortEntityHeaderHandleReference = ViewPortEntityHeader.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
        }

        internal override void WritePostData(BitWriter writer)
        {
            writer.Write_H(_viewPortEntityHeaderHandleReference);
        }
    }
}
