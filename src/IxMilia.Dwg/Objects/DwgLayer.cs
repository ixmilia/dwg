using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgLayer : DwgObject
    {
        internal override IEnumerable<DwgObject> ChildItems => new DwgObject[0];
    }
}
