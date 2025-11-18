namespace IxMilia.Dwg.Objects
{
    public partial class DwgDimensionAngular2Line: DwgDimension
    {
        // only exists for object creation during parsing
        internal DwgDimensionAngular2Line()
            : this(new DwgDimStyle(), new DwgBlock())
        {
        }

        public DwgDimensionAngular2Line(DwgDimStyle dimensionStyle, DwgBlock anonymousBlock)
            : base(dimensionStyle, anonymousBlock)
        {
            SetDefaults();
        }
    }
}
