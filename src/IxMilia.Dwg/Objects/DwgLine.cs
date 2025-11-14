#nullable enable

namespace IxMilia.Dwg.Objects
{
    public partial class DwgLine
    {
        public DwgLine(DwgPoint p1, DwgPoint p2)
            : this()
        {
            P1 = p1;
            P2 = p2;
        }
    }
}
