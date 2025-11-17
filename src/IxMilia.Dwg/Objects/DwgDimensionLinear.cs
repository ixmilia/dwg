#nullable enable

namespace IxMilia.Dwg.Objects
{
    public partial class DwgDimensionLinear : DwgDimension
    {
        // only exists for object creation during parsing
        internal DwgDimensionLinear()
            : this(new DwgDimStyle(), new DwgBlock())
        {
        }

        public DwgDimensionLinear(DwgDimStyle dimensionStyle, DwgBlock anonymousBlock)
            : base(dimensionStyle, anonymousBlock)
        {
            SetDefaults();
        }
    }
}
