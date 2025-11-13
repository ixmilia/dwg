#nullable enable

using System;

namespace IxMilia.Dwg
{
    public class DwgReadException : Exception
    {
        public DwgReadException(string message)
            : base(message)
        {
        }
    }
}
