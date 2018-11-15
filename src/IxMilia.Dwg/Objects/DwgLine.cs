namespace IxMilia.Dwg.Objects
{
    public partial class DwgLine
    {
        public DwgLine(DwgPoint p1, DwgPoint p2)
            : this()
        {
            P1 = p1;
            P2 = p2;
        }

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
        }
    }
}
