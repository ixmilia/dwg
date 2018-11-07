using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgLayer : DwgObject
    {
        public DwgLineType LineType { get; internal set; }

        internal override void PreWrite()
        {
            _lineTypeHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, LineType.Handle.HandleOrOffset);
        }

        internal override void PoseParse(BitReader reader, DwgObjectCache objectCache)
        {
            if (LayerControlHandle.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect layer control object parent handle code.");
            }

            foreach (var reactorHandle in _reactorHandles)
            {
                if (reactorHandle.Code != DwgHandleReferenceCode.HardPointer)
                {
                    throw new DwgReadException("Incorrect reactor handle code.");
                }
            }

            if (_xDictionaryObjectHandle.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect XDictionary object handle code.");
            }

            if (_nullHandle.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect object NULL handle code.");
            }

            if (_nullHandle.HandleOrOffset != 0)
            {
                throw new DwgReadException("Incorrect object NULL handle value.");
            }

            if (_lineTypeHandle.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect layer line type handle code.");
            }

            LineType = objectCache.GetObject<DwgLineType>(reader, _lineTypeHandle.HandleOrOffset);
        }
    }
}
