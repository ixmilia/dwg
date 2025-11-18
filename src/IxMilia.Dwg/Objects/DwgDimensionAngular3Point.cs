namespace IxMilia.Dwg.Objects
{
    public partial class DwgDimensionAngular3Point: DwgDimension
    {
        // only exists for object creation during parsing
        internal DwgDimensionAngular3Point()
            : this(new DwgDimStyle(), new DwgBlock())
        {
        }

        public DwgDimensionAngular3Point(DwgDimStyle dimensionStyle, DwgBlock anonymousBlock)
            : base(dimensionStyle, anonymousBlock)
        {
            SetDefaults();
        }
    }
}
