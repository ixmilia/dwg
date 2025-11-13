#nullable enable

using System;

namespace IxMilia.Dwg
{
    public struct DwgPoint : IEquatable<DwgPoint>
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public DwgPoint(double x, double y, double z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static implicit operator DwgVector(DwgPoint point) => new DwgVector(point.X, point.Y, point.Z);

        public static bool operator ==(DwgPoint p1, DwgPoint p2) => p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z;

        public static bool operator !=(DwgPoint p1, DwgPoint p2) => !(p1 == p2);

        public static DwgPoint operator +(DwgPoint p1, DwgVector p2) => new DwgPoint(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);

        public static DwgVector operator -(DwgPoint p1, DwgVector p2) => new DwgVector(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);

        public static DwgPoint operator *(DwgPoint p, double scalar) => new DwgPoint(p.X * scalar, p.Y * scalar, p.Z * scalar);

        public static DwgPoint operator /(DwgPoint p, double scalar) => new DwgPoint(p.X / scalar, p.Y / scalar, p.Z / scalar);

        public override bool Equals(object? obj) => obj is DwgPoint && this == (DwgPoint)obj;

        public bool Equals(DwgPoint other) => this == other;

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();

        public override string ToString() => string.Format("({0},{1},{2})", X, Y, Z);

        public static DwgPoint Origin => new DwgPoint(0.0, 0.0, 0.0);
    }
}
