using System;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgAppId
    {
        public DwgAppId(string name)
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
            // according to the spec, AppIdControlHandleReference.Code should be HardPointer (4), but AutoCAD sometimes produces HandleMinus1 (8)
        }

        internal static DwgAppId GetAcadAppId()
        {
            return new DwgAppId() { Name = "ACAD" };
        }

        internal static DwgAppId GetAcadMLeaderVersionAppId()
        {
            return new DwgAppId() { Name = "ACAD_MLEADERVER" };
        }
    }
}
