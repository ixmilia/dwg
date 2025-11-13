#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IxMilia.Dwg.Objects;

namespace IxMilia.Dwg
{
    internal class DwgObjectMap
    {
        private uint _nextHandle = 1;
        private IDictionary<DwgHandle, int> _handleOffsets = new Dictionary<DwgHandle, int>();

        public int HandleCount => _handleOffsets.Count;

        internal void SetOffset(DwgHandle handle, int offset)
        {
            _handleOffsets.Add(handle, offset);
        }

        public int GetOffsetFromHandle(DwgHandle handle)
        {
            if (_handleOffsets.TryGetValue(handle, out var offset))
            {
                return offset;
            }

            throw new InvalidOperationException($"Unable to get offset from handle {handle}.");
        }

        internal void AssignHandle(DwgObject obj)
        {
            if (!obj.Handle.IsNull)
            {
                return;
            }

            var next = _nextHandle++;
            obj.Handle = new DwgHandle(next);
        }

        public void SetNextAvailableHandle(DwgHeaderVariables variables)
        {
            var next = _nextHandle++;
            variables.NextAvailableHandle = new DwgHandleReference(DwgHandleReferenceCode.Declaration, next);
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
            uint lastHandle = 0;
            var lastLocation = 0;
            var ms = new MemoryStream();
            var sectionWriter = new BitWriter(ms);
            foreach (var kvp in _handleOffsets.OrderBy(k => k.Key))
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
                var handleDiff = (uint)kvp.Key - lastHandle;
                var locationDiff = kvp.Value - lastLocation;
                sectionWriter.Write_MC((int)handleDiff);
                sectionWriter.Write_MC(locationDiff);
                lastHandle = (uint)kvp.Key;
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
