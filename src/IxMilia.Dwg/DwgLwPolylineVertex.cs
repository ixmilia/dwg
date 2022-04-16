using System;

namespace IxMilia.Dwg
{
    public struct DwgLwPolylineVertex : IEquatable<DwgLwPolylineVertex>
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double StartWidth { get; set; }
        public double EndWidth { get; set; }
        public double Bulge { get; set; }

        public DwgLwPolylineVertex(double x, double y, double startWidth, double endWidth, double bulge)
            : this()
        {
            X = x;
            Y = y;
            StartWidth = startWidth;
            EndWidth = endWidth;
            Bulge = bulge;
        }

        public static bool operator ==(DwgLwPolylineVertex a, DwgLwPolylineVertex b)
        {
            return a.X == b.X
                && a.Y == b.Y
                && a.StartWidth == b.StartWidth
                && a.EndWidth == b.EndWidth
                && a.Bulge == b.Bulge;
        }

        public static bool operator !=(DwgLwPolylineVertex a, DwgLwPolylineVertex b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is DwgLwPolylineVertex vertex && this == vertex;
        }

        public bool Equals(DwgLwPolylineVertex other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            int hashCode = 1760853542;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + StartWidth.GetHashCode();
            hashCode = hashCode * -1521134295 + EndWidth.GetHashCode();
            hashCode = hashCode * -1521134295 + Bulge.GetHashCode();
            return hashCode;
        }
    }
}
