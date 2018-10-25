using System;
﻿using System.Diagnostics;

namespace IxMilia.Dwg
{
    public partial class DwgHeaderVariables
    {
        internal static byte[] StartSentinel = new byte[]
        {
            0xCF, 0x7B, 0x1F, 0x23, 0xFD, 0xDE, 0x38, 0xA9,
            0x5F, 0x7C, 0x68, 0xB8, 0x4E, 0x6D, 0x33, 0x5F
        };

        internal static byte[] EndSentinel = new byte[]
        {
            0x30, 0x84, 0xE0, 0xDC, 0x02, 0x21, 0xC7, 0x56,
            0xA0, 0x83, 0x97, 0x47, 0xB1, 0x92, 0xCC, 0xA0
        };

        internal static DwgHeaderVariables Parse(BitReader reader, DwgVersionId version)
        {
            var header = new DwgHeaderVariables();
            reader.AssertSentinel(StartSentinel);
            var dataStartOffset = reader.Offset;

            var size = reader.Read_RL();
            var startOffset = reader.Offset;
            header.ReadVariables(reader, version);
            reader.AlignToByte();
            var unreadByteCount = Math.Max(startOffset + size - reader.Offset, 0);
            var unreadBytes = reader.ReadBytes(unreadByteCount);

            reader.AlignToByte();
            var dataEndOffset = reader.Offset;
            var reportedCrc = (ushort)reader.Read_RS();

            // according to the spec an initial CRC value of 0xC0C1 is to be used here
            var computedCrc = BitReaderExtensions.ComputeCRC(reader.Data, dataStartOffset, dataEndOffset - dataStartOffset, 0xC0C1);
            Debug.Assert(computedCrc == reportedCrc);
            reader.AssertSentinel(EndSentinel);
            return header;
        }
    }

    public enum DwgAngleDirection
    {
        CounterClockwise = 0,
        Clockwise = 1
    }

    public enum DwgAngleFormat
    {
        DecimalDegrees = 0,
        DegreesMinutesSeconds = 1,
        Gradians = 2,
        Radians = 3,
        SurveyorsUnits = 4
    }

    public enum DwgAttributeVisibility
    {
        None = 0,
        Normal = 1,
        All = 2
    }

    public enum DwgCoordinateDisplay
    {
        Static = 0,
        ContinuousUpdate = 1,
        DistanceAngleFormat = 2
    }

    public enum DwgDimensionFit
    {
        TextAndArrowsOutsideLines = 0,
        MoveArrowsFirst = 1,
        MoveTextFirst = 2,
        MoveEitherForBestFit = 3
    }

    public enum DwgDimensionTextJustification
    {
        AboveLineCenter = 0,
        AboveLineNextToFirstExtension = 1,
        AboveLineNextToSecondExtension = 2,
        AboveLineCenteredOnFirstExtension = 3,
        AboveLineCenteredOnSecondExtension = 4
    }

    public enum DwgDragMode
    {
        Off = 0,
        On = 1,
        Auto = 2
    }

    public enum DwgJustification
    {
        Top = 0,
        Middle = 1,
        Bottom = 2
    }

    public enum DwgPickStyle
    {
        None = 0,
        Group = 1,
        AssociativeHatch = 2,
        GroupAndAssociativeHatch = 3
    }

    public enum DwgPolylineCurvedAndSmoothSurfaceType
    {
        None = 0,
        QuadraticBSpline = 5,
        CubicBSpline = 6,
        Bezier = 8
    }

    public enum DwgPolySketchMode
    {
        SketchLines = 0,
        SketchPolylines = 1
    }

    public enum DwgShadeEdgeMode
    {
        FacesShadedEdgeNotHighlighted = 0,
        FacesShadedEdgesHighlightedInBlack = 1,
        FacesNotFilledEdgesInEntityColor = 2,
        FacesInEntityColorEdgesInBlack = 3
    }

    public enum DwgUnitFormat
    {
        Scientific = 1,
        Decimal = 2,
        Engineering = 3,
        ArchitecturalStacked = 4,
        FractionalStacked = 5,
        Architectural = 6,
        Fractional = 7,
    }

    public enum DwgUnitZeroSuppression
    {
        SuppressZeroFeetAndZeroInches = 0,
        IncludeZeroFeetAndZeroInches = 1,
        IncludeZeroFeetAndSuppressZeroInches = 2,
        IncludeZeroInchesAndSuppressZeroFeet = 3
    }
}
