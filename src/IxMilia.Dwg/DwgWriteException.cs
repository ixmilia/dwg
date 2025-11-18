using System;

namespace IxMilia.Dwg
{
    public class DwgWriteException : Exception
    {
        public DwgWriteException(string message)
            : base(message)
        {
        }
    }
}
