using System;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgDimStyle
    {
        public DwgStyle Style { get; set; }

        internal override IEnumerable<DwgObject> ChildItems
        {
            get
            {
                yield return Style;
            }
        }

        internal override DwgHandleReferenceCode ExpectedNullHandleCode => DwgHandleReferenceCode.SoftOwner;

        public DwgDimStyle(string name)
            : this()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Name cannot be null.");
            }

            Name = name;
        }

        internal override void PreWrite()
        {
            _styleHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, Style.Handle.HandleOrOffset);
        }

        internal override void PoseParse(BitReader reader, DwgObjectCache objectCache)
        {
            base.PoseParse(reader, objectCache);
            if (DimStyleControlHandle.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect style control object parent handle code.");
            }

            foreach (var reactorHandle in _reactorHandles)
            {
                if (reactorHandle.Code != DwgHandleReferenceCode.HardPointer)
                {
                    throw new DwgReadException("Incorrect reactor handle code.");
                }
            }

            if (_styleHandle.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect style handle code.");
            }

            Style = objectCache.GetObject<DwgStyle>(reader, _styleHandle.HandleOrOffset);
        }

        internal static DwgDimStyle GetStandardDimStyle(DwgStyle style)
        {
            return new DwgDimStyle()
            {
                Name = "STANDARD",
                Style = style
            };
        }
    }
}
