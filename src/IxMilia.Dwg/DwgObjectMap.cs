using IxMilia.Dwg.Objects;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IxMilia.Dwg
{
    internal class DwgObjectMap
    {
        private int _nextHandle = 1;
        private IDictionary<int, int> _handleOffsets = new Dictionary<int, int>();

        public int GetOffset(int handle)
        {
            if (_handleOffsets.TryGetValue(handle, out var offset))
            {
                return offset;
            }

            throw new DwgReadException($"Handle {handle} not found in object map.");
        }

        internal void SetOffset(int handle, int offset)
        {
            _handleOffsets.Add(handle, offset);
        }

        internal void AssignHandle(DwgObject obj)
        {
            if (!obj.Handle.IsEmpty)
            {
                return;
            }

            var next = _nextHandle++;
            obj.Handle = new DwgHandleReference(DwgHandleReferenceCode.Declaration, next);
        }

        public static DwgObjectMap Parse(BitReader reader)
        {
            var objectMap = new DwgObjectMap();
            var lastHandle = 0;
            var lastLocation = 0;
            reader.StartCrcCheck();
            var sectionSize = reader.ReadShortBigEndian();
            while (sectionSize != 2)
            {
                var sectionStart = reader.Offset;
                var sectionEnd = sectionStart + sectionSize - 2;
                while (reader.Offset < sectionEnd)
                {
                    // read data
                    var handleOffset = reader.Read_MC();
                    var locationOffset = reader.Read_MC();
                    var handle = lastHandle + handleOffset;
                    var location = lastLocation + locationOffset;
                    objectMap._handleOffsets[handle] = location;
                    lastHandle = handle;
                    lastLocation = location;
                }

                reader.ValidateCrc(initialValue: DwgHeaderVariables.InitialCrcValue, readCrcAsMsb: true);
                reader.StartCrcCheck();
                sectionSize = reader.ReadShortBigEndian();
            }

            reader.ValidateCrc(initialValue: DwgHeaderVariables.InitialCrcValue, readCrcAsMsb: true);
            return objectMap;
        }

        private const int MaxSectionSize = 2032;
        private const int MaxSectionSizeBuffer = 10; // the maximum size of 2 MC values

        public void Write(BitWriter writer)
        {
            void WriteSection(byte[] data)
            {
                writer.StartCrcCalculation(initialValue: DwgHeaderVariables.InitialCrcValue);
                writer.WriteShortBigEndian((short)(data.Length + 2)); // account for crc
                writer.WriteBytes(data);
                writer.WriteCrc(writeCrcAsMsb: true);
            }

            // write in sections, each section is at most 2032 bytes, stop writing if remaining space is less than 10
            var sectionStart = writer.Position;
            var lastHandle = 0;
            var lastLocation = 0;
            var ms = new MemoryStream();
            var sectionWriter = new BitWriter(ms);
            foreach (var kvp in _handleOffsets)
            {
                var sectionSize = writer.Position - sectionStart;
                if (sectionSize >= MaxSectionSize - MaxSectionSizeBuffer)
                {
                    // flush section
                    var sectionBytes = sectionWriter.AsBytes();
                    WriteSection(sectionBytes);
                    ms.Dispose();
                    ms = new MemoryStream();
                    sectionWriter = new BitWriter(ms);
                    sectionStart = writer.Position;
                }

                // now write the values
                var handleDiff = kvp.Key - lastHandle;
                var locationDiff = kvp.Value - lastLocation;
                sectionWriter.Write_MC(handleDiff);
                sectionWriter.Write_MC(locationDiff);
                lastHandle = kvp.Key;
                lastLocation = kvp.Value;
            }

            // flush the final section
            var finalBytes = sectionWriter.AsBytes();
            if (finalBytes.Length > 0)
            {
                WriteSection(finalBytes);
            }

            ms.Dispose();

            // always finish with an empty section
            WriteSection(new byte[0]);
        }
    }
}
