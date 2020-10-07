using System;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgLayer : DwgObject
    {
        public DwgLineType LineType { get; internal set; }

        internal override IEnumerable<DwgObject> ChildItems
        {
            get
            {
                yield return LineType;
            }
        }

        internal override DwgHandleReferenceCode ExpectedNullHandleCode => DwgHandleReferenceCode.SoftOwner;

        public DwgLayer(string name)
            : this()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Name cannot be null.");
            }

            Name = name;
        }

        internal override void OnBeforeObjectWrite()
        {
            base.OnBeforeObjectWrite();
            _lineTypeHandleReference = LineType.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache)
        {
            if (LayerControlHandleReference.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect layer control object parent handle code.");
            }

            if (_lineTypeHandleReference.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect layer line type handle code.");
            }

            LineType = objectCache.GetObject<DwgLineType>(reader, ResolveHandleReference(_lineTypeHandleReference));
        }
    }
}
