using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgDimStyle
    {
        public const string XDataStyleName = "DSTYLE";

        public DwgStyle DimensionTextStyle { get; set; }

        internal override IEnumerable<DwgObject> ChildItems
        {
            get
            {
                yield return DimensionTextStyle;
            }
        }

        internal override DwgHandleReferenceCode ExpectedNullHandleCode => DwgHandleReferenceCode.SoftOwner;

        // only exists for object creation during parsing
        internal DwgDimStyle()
            : this("UnnamedDimStyle", new DwgStyle("STANDARD"))
        {
        }

        public DwgDimStyle(string name, DwgStyle dimensionTextStyle)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Name cannot be null.");
            }

            SetDefaults();

            Name = name;
            DimensionTextStyle = dimensionTextStyle;
        }

        internal override void OnBeforeObjectWrite(DwgVersionId version)
        {
            base.OnBeforeObjectWrite(version);
            _dimensionTextStyleHandleReference = DimensionTextStyle.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            // according to the spec, DimStyleControlHandleReference.Code should be HardPointer (4), but AutoCAD sometimes produces HandleMinus1 (8)

            if (_dimensionTextStyleHandleReference.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect style handle code.");
            }

            DimensionTextStyle = objectCache.GetObject<DwgStyle>(reader, ResolveHandleReference(_dimensionTextStyleHandleReference));
        }

        public bool TryGetStyleFromXDataDifference(DwgXDataItemList xdataItemList, [NotNullWhen(returnValue: true)] out DwgDimStyle? style)
        {
            // style data is encoded as
            //   0 DSTYLE
            //     {
            //     ... style overrides
            //     }

            style = null;
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

        internal static DwgDimStyle GetStandardDimStyle(DwgStyle dimensionTextStyle)
        {
            return new DwgDimStyle("STANDARD", dimensionTextStyle);
        }
    }
}
