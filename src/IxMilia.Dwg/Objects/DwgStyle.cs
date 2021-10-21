using System;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgStyle : DwgObject
    {
        public DwgStyle(string name)
            : this()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Name cannot be null.");
            }

            Name = name;
        }

        internal override DwgHandleReferenceCode ExpectedNullHandleCode => DwgHandleReferenceCode.SoftOwner;

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            if (StyleControlHandleReference.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect style control object parent handle code.");
            }
        }
    }
}
