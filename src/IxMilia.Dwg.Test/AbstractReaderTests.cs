using System;

namespace IxMilia.Dwg.Test
{
    public abstract class AbstractReaderTests
    {
        internal static BitReader Bits(params int[] data)
        {
            var bytes = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] < 0 || data[i] > 255)
                {
                    // the int data type is only a convenience
                    throw new NotSupportedException("Byte values must be between [0, 255].");
                }

                bytes[i] = (byte)data[i];
            }

            return new BitReader(bytes);
        }
    }
}
