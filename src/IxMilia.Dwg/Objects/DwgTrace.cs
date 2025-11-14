#nullable enable

using System;
using System.Linq;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgTrace
    {
        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            FirstCorner = new DwgPoint(FirstCorner.X, FirstCorner.Y, Elevation);
            SecondCorner = new DwgPoint(SecondCorner.X, SecondCorner.Y, Elevation);
            ThirdCorner = new DwgPoint(ThirdCorner.X, ThirdCorner.Y, Elevation);
            FourthCorner = new DwgPoint(FourthCorner.X, FourthCorner.Y, Elevation);
        }

        internal override void OnBeforeEntityWrite(DwgVersionId version)
        {
            var elevationValues = new[] { FirstCorner.Z, SecondCorner.Z, ThirdCorner.Z, FourthCorner.Z };
            var uniqueElevationValues = elevationValues.Distinct().ToList();
            if (uniqueElevationValues.Count != 1)
            {
                throw new InvalidOperationException("The Z-value of all corners must be the same.");
            }

            Elevation = uniqueElevationValues[0];
        }
    }
}
