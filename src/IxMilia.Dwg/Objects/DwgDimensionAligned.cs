#nullable enable

namespace IxMilia.Dwg.Objects
{
    public partial class DwgDimensionAligned: DwgDimension
    {
        // only exists for object creation during parsing
        internal DwgDimensionAligned()
            : this(new DwgDimStyle(), new DwgBlock())
        {
        }

        public DwgDimensionAligned(DwgDimStyle dimensionStyle, DwgBlock anonymousBlock)
            : base(dimensionStyle, anonymousBlock)
        {
            SetDefaults();
        }
    }
}
