namespace IxMilia.Dwg.Objects
{
    public abstract class DwgEntity : DwgObject
    {
        internal override bool IsEntity => true;

        protected bool _isGraphicPresent;
        protected int _graphicsSize;
        protected byte[] _graphicsData;
        protected int _entityMode;
        protected bool _isLineTypeByLayer;
        protected bool _noLinks;
        public DwgColor Color { get; set; }
        public double LineTypeScale { get; set; }
        protected short _invisibility;
        internal DwgHandleReference LayerHandle { get; set; }
    }
}
