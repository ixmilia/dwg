#nullable enable

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
                throw new ArgumentNullException(nameof(name), "Name cannot be empty.");
            }

            Name = name;
        }

        internal override DwgHandleReferenceCode ExpectedNullHandleCode => DwgHandleReferenceCode.SoftOwner;

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            // according to the spec, StyleControlHandleReference.Code should be HardPointer (4), but AutoCAD sometimes produces HandleMinus1 (8)
        }
    }
}
