namespace IxMilia.Dwg.Objects
{
    public partial class DwgDimensionOrdinate : DwgDimension
    {
        // only exists for object creation during parsing
        internal DwgDimensionOrdinate()
            : this(new DwgDimStyle(), new DwgBlock())
        {
        }

        public DwgDimensionOrdinate(DwgDimStyle dimensionStyle, DwgBlock anonymousBlock)
            : base(dimensionStyle, anonymousBlock)
        {
            SetDefaults();
        }
    }
}
