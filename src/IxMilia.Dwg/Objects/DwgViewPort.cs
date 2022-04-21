using System;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgViewPort
    {
        public DwgViewPort(string name)
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
            // according to the spec, ViewPortControlHandleReference.Code should be HardPointer (4), but AutoCAD sometimes produces HandleMinus1 (8)
        }

        internal static DwgViewPort GetActiveViewPort()
        {
            return new DwgViewPort()
            {
                Name = "*ACTIVE",
            };
        }
    }
}
