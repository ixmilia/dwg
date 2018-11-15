namespace IxMilia.Dwg.Objects
{
    public partial class DwgEndBlock
    {
        internal override void OnBeforeEntityWrite()
        {
            _noLinks = _subentityRef.IsEmpty;
        }

        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache)
        {
            if (!_noLinks && !_subentityRef.IsEmpty && _subentityRef.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect sub entity handle code.");
            }

            if (LayerHandle.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect layer handle code.");
            }

            if (!_isLineTypeByLayer && (_lineTypeHandle.IsEmpty || _lineTypeHandle.Code != DwgHandleReferenceCode.SoftOwner))
            {
                throw new DwgReadException("Incorrect line type handle code.");
            }
        }
    }
}
