#nullable enable

namespace IxMilia.Dwg.Objects
{
    public partial class DwgFace3D
    {
        public DwgFace3D(DwgPoint firstCorner, DwgPoint secondCorner, DwgPoint thirdCorder, DwgPoint fourthCorner)
            : this()
        {
            FirstCorner = firstCorner;
            SecondCorner = secondCorner;
            ThirdCorner = thirdCorder;
            FourthCorner = fourthCorner;
        }
    }
}
