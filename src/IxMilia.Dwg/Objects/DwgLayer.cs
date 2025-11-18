using System;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgLayer : DwgObject
    {
        public DwgLineType LineType { get; set; }

        internal override IEnumerable<DwgObject> ChildItems
        {
            get
            {
                yield return LineType;
            }
        }

        internal override DwgHandleReferenceCode ExpectedNullHandleCode => DwgHandleReferenceCode.SoftOwner;

        // only exists for object creation during parsing
        internal DwgLayer()
            : this("0", new DwgLineType())
        {
        }

        public DwgLayer(string name, DwgLineType lineType)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Name cannot be empty.");
            }

            SetDefaults();

            Name = name;
            LineType = lineType;
        }

        internal override void OnBeforeObjectWrite(DwgVersionId version)
        {
            base.OnBeforeObjectWrite(version);
            _lineTypeHandleReference = LineType.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            // according to the spec, LayerControlHandleReference.Code should be HardPointer (4), but AutoCAD sometimes produces HandleMinus1 (8)

            if (_lineTypeHandleReference.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect layer line type handle code.");
            }

            LineType = objectCache.GetObject<DwgLineType>(reader, ResolveHandleReference(_lineTypeHandleReference));
        }
    }
}
