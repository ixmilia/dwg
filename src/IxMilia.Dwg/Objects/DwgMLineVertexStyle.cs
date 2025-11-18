using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public struct DwgMLineVertexStyle
    {
        public List<double> SegmentParameters { get; } = new List<double>();
        public List<double> AreaFillParameters { get; } = new List<double>();

        public DwgMLineVertexStyle(IEnumerable<double> segmentParameters, IEnumerable<double> areaFillParameters)
            : this()
        {
            SegmentParameters.AddRange(segmentParameters);
            AreaFillParameters.AddRange(areaFillParameters);
        }
    }
}
