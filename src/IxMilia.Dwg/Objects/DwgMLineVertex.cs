#nullable enable

using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public struct DwgMLineVertex
    {
        public DwgPoint Location { get; set; }
        public DwgVector VertexDirection { get; set; }
        public DwgVector MiterDirection { get; set; }
        public List<DwgMLineVertexStyle> Styles { get; } = new List<DwgMLineVertexStyle>();

        public DwgMLineVertex(DwgPoint location, DwgVector vertexDirection, DwgVector miterDirection, IEnumerable<DwgMLineVertexStyle> styles)
            : this()
        {
            Location = location;
            VertexDirection = vertexDirection;
            MiterDirection = miterDirection;
            Styles.AddRange(styles);
        }
    }
}
