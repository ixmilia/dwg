namespace IxMilia.Dwg.Objects
{
    public partial class DwgEllipse
    {
        /// <summary>
        /// Create a new ellipse.
        /// </summary>
        /// <param name="center">The center of the ellipse.</param>
        /// <param name="majorAxis">The semi-major axis of the ellipse.</param>
        /// <param name="minorAxisRatio">The ratio of the length of the minor axis to the major axis.</param>
        /// <param name="startAngle">The start angle of the ellipse in radians.</param>
        /// <param name="endAngle">The end angle of the ellipse in radians.</param>
        public DwgEllipse(DwgPoint center, DwgVector majorAxis, double minorAxisRatio, double startAngle, double endAngle)
            : this()
        {
            Center = center;
            MajorAxis = majorAxis;
            MinorAxisRatio = minorAxisRatio;
            StartAngle = startAngle;
            EndAngle = endAngle;
        }
    }
}
