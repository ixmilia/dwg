﻿using System;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgAppId
    {
        public DwgAppId(string name)
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
            if (AppIdControlHandleReference.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect app id control object parent handle code.");
            }
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
