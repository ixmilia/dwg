namespace IxMilia.Dwg.Objects
{
    public partial class DwgDimensionRadius: DwgDimension
    {
        // only exists for object creation during parsing
        internal DwgDimensionRadius()
            : this(new DwgDimStyle(), new DwgBlock())
        {
        }

        public DwgDimensionRadius(DwgDimStyle dimensionStyle, DwgBlock anonymousBlock)
            : base(dimensionStyle, anonymousBlock)
        {
            SetDefaults();
        }
    }
}
