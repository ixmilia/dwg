using System;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgDimStyle
    {
        public const string XDataStyleName = "DSTYLE";

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

        internal override void OnBeforeObjectWrite(DwgVersionId version)
        {
            base.OnBeforeObjectWrite(version);
            _styleHandleReference = Style.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            if (DimStyleControlHandleReference.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect style control object parent handle code.");
            }

            if (_styleHandleReference.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect style handle code.");
            }

            Style = objectCache.GetObject<DwgStyle>(reader, ResolveHandleReference(_styleHandleReference));
        }

        public bool TryGetStyleFromXDataDifference(DwgXDataItemList xdataItemList, out DwgDimStyle style)
        {
            // style data is encoded as
            //   0 DSTYLE
            //     {
            //     ... style overrides
            //     }

            style = default(DwgDimStyle);
            if (xdataItemList == null)
            {
                return false;
            }

            for (int i = 0; i < xdataItemList.Count - 1; i++)
            {
                if (xdataItemList[i] is DwgXDataString xdataString && xdataString.Value == XDataStyleName &&
                    xdataItemList[i + 1] is DwgXDataItemList itemList)
                {
                    if (itemList.Count % 2 != 0)
                    {
                        // must be an even number
                        return false;
                    }

                    var newStyle = Clone();
                    var styleIsModified = false;
                    for (int j = 0; j < itemList.Count; j += 2)
                    {
                        if (!(itemList[j] is DwgXDataShort codeItem))
                        {
                            // must alternate between short/<data>
                            return false;
                        }

                        styleIsModified |= newStyle.ApplyStyleOverride(codeItem.Value, itemList[j + 1]);
                    }

                    if (styleIsModified)
                    {
                        style = newStyle;
                        return true;
                    }
                }
            }

            return false;
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
