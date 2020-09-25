namespace IxMilia.Dwg.Objects
{
    public partial class DwgDictionaryVar
    {
        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache)
        {
            if (_parentHandle.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect parent handle code.");
            }
        }
    }
}
