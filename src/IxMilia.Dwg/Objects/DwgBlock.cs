#nullable enable

using System;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgBlock
    {
        public DwgBlock(string name)
            : this()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Name cannot be empty.");
            }

            Name = name;
        }
    }
}
