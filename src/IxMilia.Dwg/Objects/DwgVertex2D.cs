using System;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgVertex2D
    {
        public DwgVertex2D(DwgPoint point)
            : this()
        {
            Point = point;
        }

        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            if (StartWidth < 0.0)
            {
                StartWidth = Math.Abs(StartWidth);
                EndWidth = StartWidth;
            }
        }
    }
}
