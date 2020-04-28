using System;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgVertex2D
    {
        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache)
        {
            if (StartWidth < 0.0)
            {
                StartWidth = Math.Abs(StartWidth);
                EndWidth = StartWidth;
            }
        }
    }
}
