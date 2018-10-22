using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace IxMilia.Dwg
{
    internal static class BitReaderExtensions
    {
        public static bool Read_B(this BitReader reader)
        {
            return reader.ReadBit() == 1;
        }

        public static int Read_BB(this BitReader reader)
        {
            return reader.ReadBits(2);
        }

        public static short Read_BS(this BitReader reader)
        {
            var twoBits = reader.Read_BB();
            switch (twoBits)
            {
                case 0b00:
                    return reader.ReadShort();
                case 0b01:
                    return reader.ReadByte();
                case 0b10:
                    return 0;
                default:
                    return 256;
            }
        }

        public static int Read_BL(this BitReader reader)
        {
            var twoBits = reader.Read_BB();
            switch (twoBits)
            {
                case 0b00:
                    return reader.ReadInt();
                case 0b01:
                    return reader.ReadByte();
                case 0b10:
                    return 0;
                default:
                    Debug.Assert(false, "Unsupported bit code");
                    return 256;
            }
        }

        public static double Read_BD(this BitReader reader)
        {
            var twoBits = reader.Read_BB();
            switch (twoBits)
            {
                case 0b00:
                    return reader.ReadDouble();
                case 0b01:
                    return 1.0;
                case 0b10:
                    return 0.0;
                default:
                    Debug.Assert(false, "Unsupported bit code");
                    return double.NaN;
            }
        }

        public static Tuple<double, double> Read_2BD(this BitReader reader)
        {
            return Tuple.Create(reader.Read_BD(), reader.Read_BD());
        }

        public static Tuple<double, double, double> Read_3BD(this BitReader reader)
        {
            return Tuple.Create(reader.Read_BD(), reader.Read_BD(), reader.Read_BD());
        }

        public static byte Read_RC(this BitReader reader)
        {
            return reader.ReadByte();
        }

        public static short Read_RS(this BitReader reader)
        {
            return reader.ReadShort();
        }

        public static double Read_RD(this BitReader reader)
        {
            return reader.ReadDouble();
        }

        public static int Read_RL(this BitReader reader)
        {
            return reader.ReadInt();
        }

        public static Tuple<double, double> Read_2RD(this BitReader reader)
        {
            return Tuple.Create(reader.Read_RD(), reader.Read_RD());
        }

        public static Tuple<double, double, double> Read_3RD(this BitReader reader)
        {
            return Tuple.Create(reader.Read_RD(), reader.Read_RD(), reader.Read_RD());
        }

        public static int Read_MC(this BitReader reader)
        {
            // read bytes until no high bit
            var bytes = new Stack<byte>();
            byte b;
            do
            {
                b = reader.ReadByte();
                bytes.Push((byte)(b & 0x7F)); // only take lowest 7 bits
            } while ((b & 0x80) != 0); // while high bit set

            bool negate = false;
            if ((b & 0x40) != 0)
            {
                negate = true;
                bytes.Pop();
                bytes.Push((byte)(b & 0x3F));
            }

            // re-create number
            int result = 0;
            while (bytes.Any())
            {
                result = (result << 7) + bytes.Pop();
            }

            if (negate)
            {
                result *= -1;
            }

            return result;
        }

        public static int Read_MS(this BitReader reader)
        {
            // read shorts until no high bit
            var shorts = new Stack<short>();
            short s;
            do
            {
                s = reader.ReadShort();
                shorts.Push((short)(s & 0x7FFF)); // only take the lowest 15 bits
            } while ((s & 0x8000) != 0); // while high bit set

            // re-create number
            int result = 0;
            while (shorts.Any())
            {
                result = (result << 15) + shorts.Pop();
            }

            return result;
        }

        public static DwgHandleReference Read_H(this BitReader reader)
        {
            var code = reader.ReadBits(4);
            var counter = reader.ReadBits(4);
            var offset = 0;
            for (int i = 0; i < counter; i++)
            {
                var b = reader.ReadByte();
                offset = (offset << 8) + b;
            }

            return new DwgHandleReference((DwgHandleReferenceCode)code, offset);
        }

        public static string Read_T(this BitReader reader)
        {
            var length = reader.Read_BS();
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append((char)reader.ReadByte());
            }

            return sb.ToString();
        }
    }
}
