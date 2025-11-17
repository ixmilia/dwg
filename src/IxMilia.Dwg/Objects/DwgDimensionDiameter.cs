#nullable enable

namespace IxMilia.Dwg.Objects
{
    public partial class DwgDimensionDiameter: DwgDimension
    {
        // only exists for object creation during parsing
        internal DwgDimensionDiameter()
            : this(new DwgDimStyle(), new DwgBlock())
        {
        }

        public DwgDimensionDiameter(DwgDimStyle dimensionStyle, DwgBlock anonymousBlock)
            : base(dimensionStyle, anonymousBlock)
        {
            SetDefaults();
        }
    }
}
