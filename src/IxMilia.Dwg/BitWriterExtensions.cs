using System;
using System.Collections.Generic;
using System.IO;

namespace IxMilia.Dwg
{
    internal static class BitWriterExtensions
    {
        public static BitWriter Write_B(this BitWriter writer, int value)
        {
            // shift left to align the lowest bit with the highest
            writer.WriteBit(value << 7);
            return writer;
        }

        public static BitWriter Write_B(this BitWriter writer, bool value)
        {
            return Write_B(writer, value ? 0b10000000 : 0);
        }

        public static BitWriter Write_BB(this BitWriter writer, int value)
        {
            // shift left to align the lower 2 bits to the upper 2
            writer.WriteBits(value << 6, 2);
            return writer;
        }

        public static BitWriter Write_4B(this BitWriter writer, int value)
        {
            // shift left to align the lower 4 bits to the upper 4
            writer.WriteBits(value << 4, 4);
            return writer;
        }

        public static BitWriter Write_BS(this BitWriter writer, short value)
        {
            if (value == 0)
            {
                writer.Write_BB(0b10);
            }
            else if (value == 256)
            {
                writer.Write_BB(0b11);
            }
            else if (value >= 0 && value <= 255)
            {
                writer.Write_BB(0b01);
                writer.WriteByte((byte)value);
            }
            else
            {
                writer.Write_BB(0b00);
                writer.WriteShort(value);
            }

            return writer;
        }

        public static BitWriter Write_BL(this BitWriter writer, int value)
        {
            if (value == 0)
            {
                writer.Write_BB(0b10);
            }
            else if (value >= 0 && value <= 255)
            {
                writer.Write_BB(0b01);
                writer.WriteByte((byte)value);
            }
            else
            {
                writer.Write_BB(0b00);
                writer.WriteInt(value);
            }

            return writer;
        }

        public static BitWriter Write_2BL(this BitWriter writer, Tuple<int, int> value)
        {
            writer.Write_BL(value.Item1);
            writer.Write_BL(value.Item2);
            return writer;
        }

        public static BitWriter Write_BD(this BitWriter writer, double value)
        {
            if (value == 0.0)
            {
                writer.Write_BB(0b10);
            }
            else if (value == 1.0)
            {
                writer.Write_BB(0b01);
            }
            else
            {
                writer.Write_BB(0b00);
                writer.WriteDouble(value);
            }

            return writer;
        }

        public static BitWriter Write_3BD(this BitWriter writer, Tuple<double, double, double> value)
        {
            writer.Write_BD(value.Item1);
            writer.Write_BD(value.Item2);
            writer.Write_BD(value.Item3);
            return writer;
        }

        public static BitWriter Write_RD(this BitWriter writer, double value)
        {
            writer.WriteDouble(value);
            return writer;
        }

        public static BitWriter Write_2RD(this BitWriter writer, Tuple<double, double> value)
        {
            writer.Write_RD(value.Item1);
            writer.Write_RD(value.Item2);
            return writer;
        }

        public static BitWriter Write_RC(this BitWriter writer, byte value)
        {
            writer.WriteByte(value);
            return writer;
        }

        public static BitWriter Write_Bytes(this BitWriter writer, byte[] data)
        {
            writer.WriteBytes(data);
            return writer;
        }

        public static BitWriter Write_RL(this BitWriter writer, int value)
        {
            writer.WriteInt(value);
            return writer;
        }

        public static BitWriter Write_MC(this BitWriter writer, int value)
        {
            var bytes = new List<int>();
            var isNegative = false;
            if (value < 0)
            {
                value *= -1;
                isNegative = true;
            }

            while (value > 0)
            {
                var b = value & 0b01111111;
                bytes.Add(b);
                value >>= 7;
            }

            for (int i = 0; i < bytes.Count; i++)
            {
                var b = bytes[i];
                if (i == bytes.Count - 1)
                {
                    // on last byte, set the negative bit
                    if (isNegative)
                    {
                        b |= 0x40;
                    }
                }
                else
                {
                    // more bytes remain, set high bit
                    b |= 0b10000000;
                }

                writer.WriteByte((byte)b);
            }

            return writer;
        }

        public static BitWriter Write_MS(this BitWriter writer, int value)
        {
            var shorts = new List<int>();
            while (value > 0)
            {
                var s = value & 0x7FFF;
                shorts.Add(s);
                value >>= 15;
            }

            for (int i = 0; i < shorts.Count; i++)
            {
                var s = shorts[i];
                if (i != shorts.Count - 1)
                {
                    // more bytes remain, set the high bit
                    s |= 0x8000;
                }

                writer.WriteShort((short)s);
            }

            return writer;
        }

        public static BitWriter Write_H(this BitWriter writer, DwgHandleReference handle)
        {
            var bytes = new Stack<byte>();
            var offset = handle.HandleOrOffset;
            while (offset != 0)
            {
                bytes.Push((byte)(offset & 0xFF));
                offset >>= 8;
            }

            var header = (((int)handle.Code) << 4) | (bytes.Count & 0x0F);
            writer.WriteByte((byte)header);
            writer.WriteBytes(bytes);

            return writer;
        }

        public static BitWriter Write_T(this BitWriter writer, string value)
        {
            if (value == null)
            {
                writer.Write_BS(0);
                return writer;
            }

            writer.Write_BS((short)value.Length);
            foreach (var c in value)
            {
                writer.WriteByte((byte)c);
            }

            return writer;
        }

        public static void WriteStringAscii(this BitWriter writer, string value, bool nullTerminated = true)
        {
            foreach (var c in value)
            {
                writer.WriteByte((byte)c);
            }

            if (nullTerminated)
            {
                writer.WriteByte(0);
            }
        }

        public static byte[] AsBytes(this BitWriter writer)
        {
            writer.Flush();
            writer.BaseStream.Seek(0, SeekOrigin.Begin);
            var bytes = new byte[writer.BaseStream.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)writer.BaseStream.ReadByte();
            }

            return bytes;
        }
    }
}
