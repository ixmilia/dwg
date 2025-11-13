#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace IxMilia.Dwg
{
    internal class DwgObjectFreeSpaceSection
    {
        public uint HandleCount { get; }
        public DateTime UpdateTime { get; }
        public uint ObjectDataStart { get; }
        public List<long> AdditionalValues { get; }

        public DwgObjectFreeSpaceSection(uint handleCount, DateTime updateTime, uint objectDataStart, IEnumerable<long>? additionalValues = null)
        {
            HandleCount = handleCount;
            UpdateTime = updateTime;
            ObjectDataStart = objectDataStart;
            AdditionalValues = additionalValues?.ToList() ?? new List<long>()
            {
                0x00000000_00000032,
                0x00000000_00000064,
                0x00000000_00000200,
                0x00000000_FFFFFFFF,
            };
        }

        public void Write(BitWriter writer)
        {
            writer.WriteInt(0);
            writer.WriteUInt(HandleCount);

            var pair = Converters.JulianDate(UpdateTime);
            writer.WriteInt(pair.Item1);
            writer.WriteInt(pair.Item2);

            writer.WriteUInt(ObjectDataStart);

            writer.WriteByte((byte)AdditionalValues.Count); // From spec: Number of 64-bit values that follow (ODA writes 4).

            writer.WriteUInt(0x00000032);
            writer.WriteUInt(0x00000000);
            writer.WriteUInt(0x00000064);
            writer.WriteUInt(0x00000000);
            writer.WriteUInt(0x00000200);
            writer.WriteUInt(0x00000000);
            writer.WriteUInt(0xFFFFFFFF);
            writer.WriteUInt(0x00000000);
        }

        public void Validate(uint actualHandleCount, DateTime actualUpdateTime, uint actualObjectDataStart)
        {
            if (HandleCount < actualHandleCount)
            {
                throw new DwgReadException("Free space handle count is too low.");
            }

            if (UpdateTime != actualUpdateTime)
            {
                throw new DwgReadException("Free space update time doesn't match.");
            }

            if (ObjectDataStart != actualObjectDataStart)
            {
                throw new DwgReadException("Free space object data start doesn't match.");
            }
        }

        public static DwgObjectFreeSpaceSection Parse(BitReader reader)
        {
            var unknown = reader.ReadInt();
            var handleCount = reader.ReadUInt();

            var item1 = reader.ReadInt();
            var item2 = reader.ReadInt();
            var updateTime = Converters.JulianDate(Tuple.Create(item1, item2));

            var objectDataStart = reader.ReadUInt();

            var longCount = reader.ReadByte();
            var longs = new long[longCount];
            for (int i = 0; i < longCount; i++)
            {
                var low = reader.ReadUInt();
                var high = reader.ReadUInt();
                longs[i] = (high << 32) + low;
            }

            return new DwgObjectFreeSpaceSection(handleCount, updateTime, objectDataStart, longs);
        }
    }
}
