#nullable enable

namespace IxMilia.Dwg
{
    public enum DwgAnnotationType
    {
        MText = 0,
        Tolerance = 1,
        Insert = 2,
        None = 3,
    }

    public enum DwgAttachmentPoint
    {
        TopLeft = 1,
        TopCenter = 2,
        TopRight = 3,
        MiddleLeft = 4,
        MiddleCenter = 5,
        MiddleRight = 6,
        BottomLeft = 7,
        BottomCenter = 8,
        BottomRight = 9
    }

    public enum DwgClipBoundaryType
    {
        Rectangle = 1,
        Polygon = 2,
    }

    public enum DwgCurveType
    {
        None = 0,
        QuadraticBSpline = 5,
        CubicBSpline = 6,
        Bezier = 8
    }

    public enum DwgDrawingDirection
    {
        LeftToRight = 1,
        TopToBottom = 3,
        ByStyle = 5
    }

    public enum DwgHatchPatternType
    {
        UserDefined = 0,
        Predefined = 1,
        Custom = 2
    }

    public enum DwgHatchStyle
    {
        OddParity = 0,
        OutermostAreaOnly = 1,
        EntireArea = 2
    }

    public enum DwgHorizontalTextJustification
    {
        Left = 0,
        Center = 1,
        Right = 2,
        Aligned = 3,
        Middle = 4,
        Fit = 5
    }

    public enum DwgImageResolutionUnits
    {
        None = 0,
        Millimieters = 1,
        Centimeters = 2,
        Meters = 3,
        Kilometers = 4,
        Inches = 5,
        Feet = 6,
        Yards = 7,
        Miles = 8,
    }

    public enum DwgVerticalTextJustification
    {
        Baseline = 0,
        Bottom = 1,
        Middle = 2,
        Top = 3
    }

    public enum DwgSplineType
    {
        ControlAndKnotsOnly = 1,
        FitPointsOnly = 2,
    }
}
