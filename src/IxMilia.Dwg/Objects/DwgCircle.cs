#nullable enable

namespace IxMilia.Dwg.Objects
{
    public partial class DwgCircle
    {
        /// <summary>
        /// Create a new circle.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        public DwgCircle(DwgPoint center, double radius)
            : this()
        {
            Center = center;
            Radius = radius;
        }
    }
}
