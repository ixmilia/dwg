namespace IxMilia.Dwg.Objects
{
    public struct DwgControlPoint
    {
        public DwgPoint Point { get; set; }
        public double Weight { get; set; }

        public DwgControlPoint(DwgPoint point, double weight)
        {
            Point = point;
            Weight = weight;
        }

        public DwgControlPoint(DwgPoint point)
            : this(point, 1.0)
        {
        }
    }
}
