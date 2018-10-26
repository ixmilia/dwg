﻿using System.Collections.Generic;

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
            return Write_B(writer, 0b10000000);
        }

        public static BitWriter Write_BB(this BitWriter writer, int value)
        {
            // shift left to align the lower 2 bits to the upper 2
            writer.WriteBits(value << 6, 2);
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
            writer.Write_BS((short)value.Length);
            foreach (var c in value)
            {
                writer.WriteByte((byte)c);
            }

            return writer;
        }
    }
}