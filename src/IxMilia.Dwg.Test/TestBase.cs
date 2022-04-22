using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IxMilia.Dwg.Objects;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public abstract class TestBase
    {
        public static DwgDrawing RoundTrip(DwgDrawing drawing)
        {
            using (var ms = new MemoryStream())
            {
                drawing.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return DwgDrawing.Load(ms);
            }
        }

        public static DwgEntity RoundTrip(DwgEntity entity)
        {
            var drawing = new DwgDrawing();
            entity.Layer = drawing.CurrentLayer;
            drawing.ModelSpaceBlockRecord.Entities.Add(entity);
            var roundTrippedDrawing = RoundTrip(drawing);
            return roundTrippedDrawing.ModelSpaceBlockRecord.Entities.Single();
        }

        public static DwgObject RoundTrip(DwgObject obj)
        {
            var drawing = new DwgDrawing();
            drawing.NamedObjectDictionary["the-object"] = obj;
            var className = DwgObjectTypeExtensions.ClassNameFromTypeCode(obj.Type);
            if (className != null)
            {
                drawing.Classes.Add(new DwgClassDefinition(0, 0, "", "", className, false, false));
            }

            var roundTrippedDrawing = RoundTrip(drawing);
            return roundTrippedDrawing.NamedObjectDictionary["the-object"];
        }

        public static void AssertEquivalent(DwgEntity entity1, DwgEntity entity2)
        {
            Assert.Equal(entity1.Color, entity2.Color);
            Assert.Equal(entity1.Layer.Name, entity2.Layer.Name);
            Assert.Equal(entity1.LineType?.Name, entity2.LineType?.Name);
            Assert.Equal(entity1.LineTypeScale, entity1.LineTypeScale);
            switch ((entity1, entity2))
            {
                case (DwgArc arc1, DwgArc arc2):
                    Assert.Equal(arc1.Center, arc2.Center);
                    Assert.Equal(arc1.Radius, arc2.Radius);
                    Assert.Equal(arc1.StartAngle, arc2.StartAngle);
                    Assert.Equal(arc1.EndAngle, arc2.EndAngle);
                    Assert.Equal(arc1.Thickness, arc2.Thickness);
                    break;
                case (DwgCircle circle1, DwgCircle circle2):
                    Assert.Equal(circle1.Center, circle2.Center);
                    Assert.Equal(circle1.Radius, circle2.Radius);
                    Assert.Equal(circle1.Thickness, circle2.Thickness);
                    break;
                case (DwgEllipse ellipse1, DwgEllipse ellipse2):
                    Assert.Equal(ellipse1.Center, ellipse2.Center);
                    Assert.Equal(ellipse1.MajorAxis, ellipse2.MajorAxis);
                    Assert.Equal(ellipse1.MinorAxisRatio, ellipse2.MinorAxisRatio);
                    Assert.Equal(ellipse1.StartAngle, ellipse2.StartAngle);
                    Assert.Equal(ellipse1.EndAngle, ellipse2.EndAngle);
                    break;
                case (DwgLine line1, DwgLine line2):
                    Assert.Equal(line1.P1, line2.P1);
                    Assert.Equal(line1.P2, line2.P2);
                    break;
                case (DwgLocation location1, DwgLocation location2):
                    Assert.Equal(location1.Point, location2.Point);
                    break;
                case (DwgLwPolyline lwpolyline1, DwgLwPolyline lwpolyline2):
                    Assert.Equal(lwpolyline1.Width, lwpolyline2.Width);
                    Assert.Equal(lwpolyline1.Elevation, lwpolyline2.Elevation);
                    Assert.Equal(lwpolyline1.Thickness, lwpolyline2.Thickness);
                    Assert.Equal(lwpolyline1.Normal, lwpolyline2.Normal);
                    Assert.Equal(lwpolyline1.Vertices, lwpolyline2.Vertices);
                    break;
                case (DwgPolyline2D polyline2D, DwgLwPolyline lwpolyline):
                    // AutoCAD sometimes converts polyline2d entities to lwpolyline, so we have to perform an equivalence check
                    Assert.Equal(polyline2D.StartWidth, lwpolyline.Width);
                    Assert.Equal(polyline2D.EndWidth, lwpolyline.Width);
                    Assert.Equal(polyline2D.Elevation, lwpolyline.Elevation);
                    Assert.Equal(polyline2D.Thickness, lwpolyline.Thickness);
                    Assert.Equal(polyline2D.Vertices.Count, lwpolyline.Vertices.Count);
                    for (int i = 0; i < polyline2D.Vertices.Count; i++)
                    {
                        Assert.Equal(polyline2D.Vertices[i].Point.X, lwpolyline.Vertices[i].X);
                        Assert.Equal(polyline2D.Vertices[i].Point.Y, lwpolyline.Vertices[i].Y);
                        Assert.Equal(polyline2D.Vertices[i].StartWidth, lwpolyline.Vertices[i].StartWidth);
                        Assert.Equal(polyline2D.Vertices[i].EndWidth, lwpolyline.Vertices[i].EndWidth);
                        Assert.Equal(polyline2D.Vertices[i].Bulge, lwpolyline.Vertices[i].Bulge);
                    }
                    break;
                case (DwgPolyline2D polyline2D1, DwgPolyline2D polyline2D2):
                    Assert.Equal(polyline2D1.Flags, polyline2D2.Flags);
                    Assert.Equal(polyline2D1.CurveType, polyline2D2.CurveType);
                    Assert.Equal(polyline2D1.StartWidth, polyline2D2.StartWidth);
                    Assert.Equal(polyline2D1.EndWidth, polyline2D2.EndWidth);
                    Assert.Equal(polyline2D1.Thickness, polyline2D2.Thickness);
                    Assert.Equal(polyline2D1.Elevation, polyline2D2.Elevation);
                    Assert.Equal(polyline2D1.Extrusion, polyline2D2.Extrusion);
                    Assert.Equal(polyline2D1.Vertices.Count, polyline2D2.Vertices.Count);
                    for (int i = 0; i < polyline2D1.Vertices.Count; i++)
                    {
                        AssertEquivalent(polyline2D1.Vertices[i], polyline2D2.Vertices[i]);
                    }
                    break;
                case (DwgPolyline3D polyline3D1, DwgPolyline3D polyline3D2):
                    Assert.Equal(polyline3D1.Vertices.Count, polyline3D2.Vertices.Count);
                    for (int i = 0; i < polyline3D1.Vertices.Count; i++)
                    {
                        AssertEquivalent(polyline3D1.Vertices[i], polyline3D2.Vertices[i]);
                    }
                    break;
                case (DwgSpline spline1, DwgSpline spline2):
                    Assert.Equal(spline1.Degree, spline2.Degree);
                    Assert.Equal(spline1.FitTolerance, spline2.FitTolerance);
                    Assert.Equal(spline1.StartTangentVector, spline2.StartTangentVector);
                    Assert.Equal(spline1.EndTangentVector, spline2.EndTangentVector);
                    Assert.Equal(spline1.IsRational, spline2.IsRational);
                    Assert.Equal(spline1.IsClosed, spline2.IsClosed);
                    Assert.Equal(spline1.IsPeriodic, spline2.IsPeriodic);
                    Assert.Equal(spline1.KnotTolerance, spline2.KnotTolerance);
                    Assert.Equal(spline1.ControlTolerance, spline2.ControlTolerance);
                    Assert.Equal(spline1.SplineType, spline2.SplineType);
                    Assert.Equal(spline1.FitPoints, spline2.FitPoints);
                    Assert.Equal(spline1.KnotValues, spline2.KnotValues);
                    Assert.Equal(spline1.ControlPoints, spline2.ControlPoints);
                    break;
                case (DwgText text1, DwgText text2):
                    Assert.Equal(text1.Elevation, text2.Elevation);
                    Assert.Equal(text1.InsertionPoint, text2.InsertionPoint);
                    Assert.Equal(text1.AlignmentPoint, text2.AlignmentPoint);
                    Assert.Equal(text1.Extrusion, text2.Extrusion);
                    Assert.Equal(text1.Thickness, text2.Thickness);
                    Assert.Equal(text1.ObliqueAngle, text2.ObliqueAngle);
                    Assert.Equal(text1.RotationAngle, text2.RotationAngle);
                    Assert.Equal(text1.Height, text2.Height);
                    Assert.Equal(text1.WidthFactor, text2.WidthFactor);
                    Assert.Equal(text1.Value, text2.Value);
                    Assert.Equal(text1.HorizontalAlignment, text2.HorizontalAlignment);
                    Assert.Equal(text1.VerticalAlignment, text2.VerticalAlignment);
                    Assert.Equal(text1.IsTextBackward, text2.IsTextBackward);
                    Assert.Equal(text1.IsTextUpsideDown, text2.IsTextUpsideDown);
                    break;
                case (DwgVertex2D vertex2D1, DwgVertex2D vertex2D2):
                    Assert.Equal(vertex2D1.Flags, vertex2D2.Flags);
                    Assert.Equal(vertex2D1.Point, vertex2D2.Point);
                    Assert.Equal(vertex2D1.StartWidth, vertex2D2.StartWidth);
                    Assert.Equal(vertex2D1.EndWidth, vertex2D2.EndWidth);
                    Assert.Equal(vertex2D1.Bulge, vertex2D2.Bulge);
                    Assert.Equal(vertex2D1.TangentDirection, vertex2D2.TangentDirection);
                    break;
                case (DwgVertex3D vertex3D1, DwgVertex3D vertex3D2):
                    Assert.Equal(vertex3D1.Flags, vertex3D2.Flags);
                    Assert.Equal(vertex3D1.Point, vertex3D2.Point);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported entity comparison: {entity1.GetType().Name}/{entity2.GetType().Name}");
            }
        }

        public static IEnumerable<DwgEntity> GetTestEntities()
        {
            yield return new DwgArc(new DwgPoint(0.0, 0.0, 0.0), 1.0, 0.0, Math.PI);
            // TODO: block
            yield return new DwgCircle(new DwgPoint(1.0, 1.0, 0.0), 1.0);
            yield return new DwgEllipse(new DwgPoint(0.0, 2.0, 0.0), new DwgVector(1.0, 0.0, 0.0), 0.5, 0.0, Math.PI * 2.0);
            // TODO: image
            yield return new DwgLine(new DwgPoint(0.0, 0.0, 0.0), new DwgPoint(1.0, 1.0, 0.0));
            yield return new DwgLocation(new DwgPoint(2.0, 2.0, 0.0));
            yield return new DwgLwPolyline(new[]
            {
                new DwgLwPolylineVertex(0.0, 0.0, 0.0, 0.0, 0.0),
                new DwgLwPolylineVertex(2.0, 0.0, 0.0, 0.0, 0.0),
                new DwgLwPolylineVertex(2.0, 1.0, 0.0, 0.0, 0.0),
            });
            yield return new DwgPolyline2D(
                new DwgVertex2D(new DwgPoint(0.0, 0.0, 0.0)),
                new DwgVertex2D(new DwgPoint(0.0, -1.0, 0.0)),
                new DwgVertex2D(new DwgPoint(-1.0, -1.0, 0.0))
            );
            yield return new DwgPolyline3D(
                new DwgVertex3D(new DwgPoint(0.0, 0.0, 0.0)),
                new DwgVertex3D(new DwgPoint(0.0, -1.0, 0.0)),
                new DwgVertex3D(new DwgPoint(-1.0, -1.0, 0.0))
            );
            var spline = new DwgSpline()
            {
                Degree = 3,
                SplineType = DwgSplineType.ControlAndKnotsOnly,
            };
            spline.ControlPoints.Add(new DwgControlPoint(new DwgPoint(3.0, 0.0, 0.0)));
            spline.ControlPoints.Add(new DwgControlPoint(new DwgPoint(4.0, 0.0, 0.0)));
            spline.ControlPoints.Add(new DwgControlPoint(new DwgPoint(4.0, 0.0, 0.0)));
            spline.ControlPoints.Add(new DwgControlPoint(new DwgPoint(4.0, 1.0, 0.0)));
            spline.KnotValues.Add(0.0);
            spline.KnotValues.Add(0.0);
            spline.KnotValues.Add(0.0);
            spline.KnotValues.Add(0.0);
            spline.KnotValues.Add(1.0);
            spline.KnotValues.Add(1.0);
            spline.KnotValues.Add(1.0);
            spline.KnotValues.Add(1.0);
            yield return spline;
            yield return new DwgText("abcd")
            {
                Height = 1.0,
                InsertionPoint = new DwgPoint(0.0, -1.0, 0.0),
            };
        }
    }
}
