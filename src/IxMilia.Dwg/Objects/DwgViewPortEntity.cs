#nullable enable

using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgViewPortEntity
    {
        public DwgViewPortEntityHeader ViewPortEntityHeader { get; set; } = new DwgViewPortEntityHeader();

        internal DwgHandleReference _viewPortEntityHeaderHandleReference;

        internal override IEnumerable<DwgObject> ChildItems
        {
            get
            {
                yield return ViewPortEntityHeader;
            }
        }

        internal override void ReadPostData(BitReader reader, DwgVersionId version)
        {
            _viewPortEntityHeaderHandleReference = reader.Read_H();
            if (_viewPortEntityHeaderHandleReference.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect view port entity header handle code.");
            }
        }

        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            ViewPortEntityHeader = objectCache.GetObject<DwgViewPortEntityHeader>(reader, ResolveHandleReference(_viewPortEntityHeaderHandleReference));
        }

        internal override void OnBeforeEntityWrite(DwgVersionId version)
        {
            _viewPortEntityHeaderHandleReference = ViewPortEntityHeader.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
        }

        internal override void WritePostData(BitWriter writer, DwgVersionId version)
        {
            writer.Write_H(_viewPortEntityHeaderHandleReference);
        }
    }
}
