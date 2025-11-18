using System;
using System.Collections.Generic;
using System.Linq;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgHatch
    {
        public abstract class BoundaryPathBase
        {
            public List<DwgHandleReference> BoundaryItemHandleReferences = new List<DwgHandleReference>();

            internal abstract void Write(BitWriter writer);
        }

        public abstract class BoundaryPathEdgeBase
        {
            internal abstract void Write(BitWriter writer);

            internal static BoundaryPathEdgeBase Read(BitReader reader)
            {
                var pathType = reader.Read_RC();
                switch (pathType)
                {
                    case 1:
                        return LineBoundaryPathEdge.ReadLine(reader);
                    case 2:
                        return CircularArcBoundaryPathEdge.ReadCircularArc(reader);
                    case 3:
                        return EllipticalArcBoundaryPathEdge.ReadEllipticalArc(reader);
                    case 4:
                        return SplineBoundaryPathEdge.ReadSpline(reader);
                    default:
                        throw new DwgReadException($"Unsupported boundary path edge type: {pathType}");
                }
            }
        }

        public class LineBoundaryPathEdge : BoundaryPathEdgeBase, IEquatable<LineBoundaryPathEdge>
        {
            public DwgPoint Start { get; set; }
            public DwgPoint End { get; set; }

            public LineBoundaryPathEdge(DwgPoint start, DwgPoint end)
            {
                Start = start;
                End = end;
            }

            internal override void Write(BitWriter writer)
            {
                writer.Write_RC(1);
                writer.Write_2RD(Converters.DoublePoint(Start));
                writer.Write_2RD(Converters.DoublePoint(End));
            }

            public override bool Equals(object? obj) => Equals(obj as LineBoundaryPathEdge);

            public bool Equals(LineBoundaryPathEdge? other)
            {
                return other is not null
                    && Start == other.Start
                    && End == other.End;
            }

            public override int GetHashCode()
            {
                int hashCode = -1676728671;
                hashCode = hashCode * -1521134295 + Start.GetHashCode();
                hashCode = hashCode * -1521134295 + End.GetHashCode();
                return hashCode;
            }

            internal static LineBoundaryPathEdge ReadLine(BitReader reader)
            {
                var start = Converters.DoublePoint(reader.Read_2RD());
                var end = Converters.DoublePoint(reader.Read_2RD());
                return new LineBoundaryPathEdge(start, end);
            }
        }

        public class CircularArcBoundaryPathEdge : BoundaryPathEdgeBase
        {
            public DwgPoint Center { get; set; }
            public double Radius { get; set; }
            public double StartAngle { get; set; }
            public double EndAngle { get; set; }
            public bool IsCounterClockwise { get; set; }

            public CircularArcBoundaryPathEdge(DwgPoint center, double radius, double startAngle, double endAngle, bool isCounterClockwise)
            {
                Center = center;
                Radius = radius;
                StartAngle = startAngle;
                EndAngle = endAngle;
                IsCounterClockwise = isCounterClockwise;
            }

            internal override void Write(BitWriter writer)
            {
                writer.Write_RC(2);
                writer.Write_2RD(Converters.DoublePoint(Center));
                writer.Write_BD(Radius);
                writer.Write_BD(StartAngle);
                writer.Write_BD(EndAngle);
                writer.Write_B(IsCounterClockwise);
            }

            internal static CircularArcBoundaryPathEdge ReadCircularArc(BitReader reader)
            {
                var center = Converters.DoublePoint(reader.Read_2RD());
                var radius = reader.Read_BD();
                var startAngle = reader.Read_BD();
                var endAngle = reader.Read_BD();
                var isCounterClockwise = reader.Read_B();
                return new CircularArcBoundaryPathEdge(center, radius, startAngle, endAngle, isCounterClockwise);
            }
        }

        public class EllipticalArcBoundaryPathEdge : BoundaryPathEdgeBase
        {
            public DwgPoint Center { get; set; }
            public DwgVector MajorAxis { get; set; }
            public double MinorAxisRatio { get; set; }
            public double StartAngle { get; set; }
            public double EndAngle { get; set; }
            public bool IsCounterClockwise { get; set; }

            public EllipticalArcBoundaryPathEdge(DwgPoint center, DwgVector majorAxis, double minorAxisRatio, double startAngle, double endAngle, bool isCounterClockwise)
            {
                Center = center;
                MajorAxis = majorAxis;
                MinorAxisRatio = minorAxisRatio;
                StartAngle = startAngle;
                EndAngle = endAngle;
                IsCounterClockwise = isCounterClockwise;
            }

            internal override void Write(BitWriter writer)
            {
                writer.Write_RC(3);
                writer.Write_2RD(Converters.DoublePoint(Center));
                writer.Write_2RD(Converters.DoubleVector(MajorAxis));
                writer.Write_BD(MinorAxisRatio);
                writer.Write_BD(StartAngle);
                writer.Write_BD(EndAngle);
                writer.Write_B(IsCounterClockwise);
            }

            internal static EllipticalArcBoundaryPathEdge ReadEllipticalArc(BitReader reader)
            {
                var center = Converters.DoublePoint(reader.Read_2RD());
                var majorAxis = Converters.DoubleVector(reader.Read_2RD());
                var minorAxisRatio = reader.Read_BD();
                var startAngle = reader.Read_BD();
                var endAngle = reader.Read_BD();
                var isCounterClockwise = reader.Read_B();
                return new EllipticalArcBoundaryPathEdge(center, majorAxis, minorAxisRatio, startAngle, endAngle, isCounterClockwise);
            }
        }

        public class SplineBoundaryPathEdge : BoundaryPathEdgeBase
        {
            public int Degree { get; set; }
            public bool IsRational { get; set; }
            public bool IsPeriodic { get; set; }
            public List<double> Knots { get; } = new List<double>();
            public List<DwgControlPoint> ControlPoints { get; } = new List<DwgControlPoint>();

            public SplineBoundaryPathEdge(int degree, bool isRational, bool isPeriodic, IEnumerable<double> knots, IEnumerable<DwgControlPoint> controlPoints)
            {
                Degree = degree;
                IsRational = isRational;
                IsPeriodic = isPeriodic;
                Knots.AddRange(knots);
                ControlPoints.AddRange(controlPoints);
            }

            internal override void Write(BitWriter writer)
            {
                writer.Write_RC(4);
                writer.Write_BL(Degree);
                writer.Write_B(IsRational);
                writer.Write_B(IsPeriodic);
                writer.Write_BL(Knots.Count);
                writer.Write_BL(ControlPoints.Count);
                foreach (var knot in Knots)
                {
                    writer.Write_BD(knot);
                }

                var writeWeights = ControlPoints.Any(cp => cp.Weight != 0.0);
                foreach (var controlPoint in ControlPoints)
                {
                    writer.Write_2RD(Converters.DoublePoint(controlPoint.Point));
                    if (writeWeights)
                    {
                        writer.Write_BD(controlPoint.Weight);
                    }
                }
            }

            internal static SplineBoundaryPathEdge ReadSpline(BitReader reader)
            {
                var degree = reader.Read_BL();
                var isRational = reader.Read_B(); // has weights
                var isPeriodic = reader.Read_B();
                var knotCount = reader.Read_BL();
                var controlPointCount = reader.Read_BL();
                var knots = new List<double>();
                for (var k = 0; k < knotCount; k++)
                {
                    knots.Add(reader.Read_BD());
                }

                var controlPoints = new List<DwgControlPoint>();
                for (var k = 0; k < controlPointCount; k++)
                {
                    var controlPoint = Converters.DoublePoint(reader.Read_2RD());
                    var weight = isRational ? reader.Read_BD() : 0.0;
                    controlPoints.Add(new DwgControlPoint(controlPoint, weight));
                }

                return new SplineBoundaryPathEdge(degree, isRational, isPeriodic, knots, controlPoints);
            }
        }

        public class NonPolylineBoundaryPath : BoundaryPathBase
        {
            public List<BoundaryPathEdgeBase> Edges { get; } = new List<BoundaryPathEdgeBase>();

            internal override void Write(BitWriter writer)
            {
                writer.Write_BL(Edges.Count);
                foreach (var edge in Edges)
                {
                    edge.Write(writer);
                }
            }

            internal static NonPolylineBoundaryPath Read(BitReader reader)
            {
                var result = new NonPolylineBoundaryPath();
                var pathSegmentCount = reader.Read_BL();
                for (var j = 0; j < pathSegmentCount; j++)
                {
                    var pathEdge = BoundaryPathEdgeBase.Read(reader);
                    result.Edges.Add(pathEdge);
                }

                return result;
            }
        }

        public struct PolylinePathVertex
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Bulge { get; set; }

            public PolylinePathVertex(double x, double y, double bulge)
            {
                X = x;
                Y = y;
                Bulge = bulge;
            }
        }

        public class PolylineBoundaryPath : BoundaryPathBase
        {
            public bool IsClosed { get; set; }
            public List<PolylinePathVertex> Vertices { get; } = new List<PolylinePathVertex>();

            public PolylineBoundaryPath(bool isClosed, IEnumerable<PolylinePathVertex> vertices)
            {
                IsClosed = isClosed;
                Vertices.AddRange(vertices);
            }

            internal override void Write(BitWriter writer)
            {
                var bulgesPresent = Vertices.Any(v => v.Bulge != 0.0);
                writer.Write_B(bulgesPresent);
                writer.Write_B(IsClosed);
                writer.Write_BL(Vertices.Count);
                foreach (var vertex in Vertices)
                {
                    var location = new DwgPoint(vertex.X, vertex.Y, 0.0);
                    writer.Write_2RD(Converters.DoublePoint(location));
                    if (bulgesPresent)
                    {
                        writer.Write_BD(vertex.Bulge);
                    }
                }
            }

            internal static PolylineBoundaryPath Read(BitReader reader)
            {
                var bulgesPresent = reader.Read_B();
                var isClosed = reader.Read_B();
                var pathSegmentCount = reader.Read_BL();
                var vertices = new List<PolylinePathVertex>();
                for (int j = 0; j < pathSegmentCount; j++)
                {
                    var point = Converters.DoublePoint(reader.Read_2RD());
                    var bulge = bulgesPresent ? reader.Read_BD() : 0.0;
                    vertices.Add(new PolylinePathVertex(point.X, point.Y, bulge));
                }

                return new PolylineBoundaryPath(isClosed, vertices);
            }
        }
    }
}
