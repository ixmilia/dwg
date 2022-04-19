namespace IxMilia.Dwg.Objects
{
    public partial class DwgArc
    {
        /// <summary>
        /// Create a new arc.
        /// </summary>
        /// <param name="center">The center of the arc.</param>
        /// <param name="radius">The radius of the arc.</param>
        /// <param name="startAngle">Arc start angle in radians.</param>
        /// <param name="endAngle">Arc end angle in radians.</param>
        public DwgArc(DwgPoint center, double radius, double startAngle, double endAngle)
            : this()
        {
            Center = center;
            Radius = radius;
            StartAngle = startAngle;
            EndAngle = endAngle;
        }
    }
}
