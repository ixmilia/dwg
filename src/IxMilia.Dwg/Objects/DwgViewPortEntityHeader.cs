using System;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgViewPortEntityHeader
    {
        public DwgViewPortEntityHeader(string name)
            : this()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Name cannot be null.");
            }

            Name = name;
        }

        internal override DwgHandleReferenceCode ExpectedNullHandleCode => DwgHandleReferenceCode.SoftOwner;

        internal override void PoseParse(BitReader reader, DwgObjectCache objectCache)
        {
            base.PoseParse(reader, objectCache);
            if (ViewPortEntityHeaderControlHandle.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect view port entity header control object parent handle code.");
            }
        }
    }
}
