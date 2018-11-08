namespace IxMilia.Dwg.Objects
{
    public abstract class DwgEntity : DwgObject
    {
        internal override bool IsEntity => true;
        internal DwgHandleReference LayerHandle { get; set; }
    }
}
