using System;

namespace IxMilia.Dwg
{
    public struct DwgVector : IEquatable<DwgVector>
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public DwgVector(double x, double y, double z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double LengthSquared => X * X + Y * Y + Z * Z;

        public double Length => Math.Sqrt(LengthSquared);

        public bool IsZeroVector => X == 0.0 && Y == 0.0 && Z == 0.0;

        public DwgVector Normalize() => this / Length;

        public DwgVector Cross(DwgVector v) => new DwgVector(Y * v.Z - Z * v.Y, Z * v.X - X * v.Z, X * v.Y - Y * v.X);

        public double Dot(DwgVector v) => X * v.X + Y * v.Y + Z * v.Z;

        public static implicit operator DwgPoint(DwgVector vector) => new DwgPoint(vector.X, vector.Y, vector.Z);

        public static DwgVector operator -(DwgVector vector) => new DwgVector(-vector.X, -vector.Y, -vector.Z);

        public static DwgVector operator +(DwgVector p1, DwgVector p2) => new DwgVector(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);

        public static DwgVector operator -(DwgVector p1, DwgVector p2) => new DwgVector(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);

        public static DwgVector operator *(DwgVector vector, double operand) => new DwgVector(vector.X * operand, vector.Y * operand, vector.Z * operand);

        public static DwgVector operator /(DwgVector vector, double operand) => new DwgVector(vector.X / operand, vector.Y / operand, vector.Z / operand);

        public static bool operator ==(DwgVector p1, DwgVector p2) => p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z;

        public static bool operator !=(DwgVector p1, DwgVector p2) => !(p1 == p2);

        public override bool Equals(object? obj) => obj is DwgVector && this == (DwgVector)obj;

        public bool Equals(DwgVector other) => this == other;

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();

        public bool IsParallelTo(DwgVector other) => Cross(other).IsZeroVector;

        public static DwgVector XAxis => new DwgVector(1.0, 0.0, 0.0);

        public static DwgVector YAxis => new DwgVector(0.0, 1.0, 0.0);

        public static DwgVector ZAxis => new DwgVector(0.0, 0.0, 1.0);

        public static DwgVector One => new DwgVector(1.0, 1.0, 1.0);

        public static DwgVector Zero => new DwgVector(0.0, 0.0, 0.0);

        public static DwgVector SixtyDegrees => new DwgVector(0.5, Math.Sqrt(3.0) * 0.5, 0);

        public override string ToString() => string.Format("({0},{1},{2})", X, Y, Z);

        public static DwgVector RightVectorFromNormal(DwgVector normal)
        {
            if (normal == XAxis)
            {
                return ZAxis;
            }

            var right = XAxis;
            var up = normal.Cross(right);
            return up.Cross(normal).Normalize();
        }

        public static DwgVector NormalFromRightVector(DwgVector right)
        {
            // these two functions are identical, but the separate name makes them easier to understand
            return RightVectorFromNormal(right);
        }
    }
}
