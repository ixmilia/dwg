using System;
using System.Collections.Generic;
using System.Linq;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgSpline
    {
        public DwgSplineType SplineType { get; set; }
        public List<DwgPoint> FitPoints { get; private set; } = new List<DwgPoint>();
        public List<double> KnotValues { get; private set; } = new List<double>();
        public List<DwgControlPoint> ControlPoints { get; private set; } = new List<DwgControlPoint>();

        internal override void WriteSpecific(BitWriter writer, DwgVersionId version)
        {
            writer.Write_BS((short)SplineType);
            writer.Write_BS((short)Degree);
            switch (SplineType)
            {
                case DwgSplineType.FitPointsOnly:
                    writer.Write_BD(FitTolerance);
                    writer.Write_3BD(Converters.TripleVector(StartTangentVector));
                    writer.Write_3BD(Converters.TripleVector(EndTangentVector));
                    writer.Write_BS((short)FitPoints.Count);
                    foreach (var fit in FitPoints)
                    {
                        writer.Write_3BD(Converters.TriplePoint(fit));
                    }
                    break;
                case DwgSplineType.ControlAndKnotsOnly:
                    writer.Write_B(IsRational);
                    writer.Write_B(IsClosed);
                    writer.Write_B(IsPeriodic);
                    writer.Write_BD(KnotTolerance);
                    writer.Write_BD(ControlTolerance);
                    writer.Write_BL(KnotValues.Count);
                    writer.Write_BL(ControlPoints.Count);
                    var isWeightPresent = ControlPoints.Select(c => c.Weight).Any(w => w != 1.0);
                    writer.Write_B(isWeightPresent);
                    foreach (var knot in KnotValues)
                    {
                        writer.Write_BD(knot);
                    }

                    foreach (var control in ControlPoints)
                    {
                        writer.Write_3BD(Converters.TriplePoint(control.Point));
                        if (isWeightPresent)
                        {
                            writer.Write_RD(control.Weight);
                        }
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported spline type value {(int)SplineType}");
            }

            _objectSize = writer.BitCount;
        }

        internal override void ParseSpecific(BitReader reader, int objectBitOffsetStart, DwgVersionId version)
        {
            SplineType = (DwgSplineType)reader.Read_BS();
            Degree = reader.Read_BS();

            switch (SplineType)
            {
                case DwgSplineType.FitPointsOnly:
                    FitTolerance = reader.Read_BD();
                    StartTangentVector = Converters.TripleVector(reader.Read_3BD());
                    EndTangentVector = Converters.TripleVector(reader.Read_3BD());
                    var fitPointCount = reader.Read_BS();
                    for (int i = 0; i < fitPointCount; i++)
                    {
                        FitPoints.Add(Converters.TriplePoint(reader.Read_3BD()));
                    }
                    break;
                case DwgSplineType.ControlAndKnotsOnly:
                    IsRational = reader.Read_B();
                    IsClosed = reader.Read_B();
                    IsPeriodic = reader.Read_B();
                    KnotTolerance = reader.Read_BD();
                    ControlTolerance = reader.Read_BD();
                    var knotCount = reader.Read_BL();
                    var controlPointCount = reader.Read_BL();
                    var isWeightPresent = reader.Read_B();
                    for (int i = 0; i < knotCount; i++)
                    {
                        KnotValues.Add(reader.Read_BD());
                    }

                    for (int i = 0; i < controlPointCount; i++)
                    {
                        var point = Converters.TriplePoint(reader.Read_3BD());
                        var weight = isWeightPresent ? reader.Read_RD() : 1.0;
                        ControlPoints.Add(new DwgControlPoint(point, weight));
                    }
                    break;
                default:
                    throw new DwgReadException($"Unsupported spline type value ${(int)SplineType}");
            }

            AssertObjectSize(reader, objectBitOffsetStart);
        }
    }
}
