using System.Collections.Generic;
using System.IO;

namespace IxMilia.Dwg
{
    internal class DwgClasses
    {
        private static readonly byte[] StartSentinel =
        {
            0x8D, 0xA1, 0xC4, 0xB8, 0xC4, 0xA9, 0xF8, 0xC5,
            0xC0, 0xDC, 0xF4, 0x5F, 0xE7, 0xCF, 0xB6, 0x8A
        };

        private static readonly byte[] EndSentinel =
        {
            0x72, 0x5E, 0x3B, 0x47, 0x3B, 0x56, 0x07, 0x3A,
            0x3F, 0x23, 0x0B, 0xA0, 0x18, 0x30, 0x49, 0x75
        };

        internal static IList<DwgClassDefinition> Parse(BitReader reader, DwgVersionId version)
        {
            reader.ValidateSentinel(StartSentinel);
            reader.StartCrcCheck();
            var sectionSize = reader.Read_RL();
            var dataStartOffset = reader.Offset;
            var dataEndOffset = dataStartOffset + sectionSize;
            var classes = new List<DwgClassDefinition>();
            while (reader.Offset < dataEndOffset - 1) // may be in the middle of a byte
            {
                classes.Add(DwgClassDefinition.Parse(reader));
            }

            reader.ValidateCrc(initialValue: DwgHeaderVariables.InitialCrcValue);
            reader.ValidateSentinel(EndSentinel);
            return classes;
        }

        internal static void Write(IList<DwgClassDefinition> classes, BitWriter writer)
        {
            writer.AlignByte();
            using (var ms = new MemoryStream())
            {
                // write the classes to memory
                var classWriter = new BitWriter(ms);
                for (int i = 0; i < classes.Count; i++)
                {
                    classes[i].Number = (short)(i + 500);
                    classes[i].Write(classWriter);
                }

                var classBytes = classWriter.AsBytes();

                // now write it all out
                writer.WriteBytes(StartSentinel);
                writer.StartCrcCalculation(initialValue: DwgHeaderVariables.InitialCrcValue);
                writer.Write_RL(classBytes.Length);
                writer.WriteBytes(classBytes);
                writer.WriteCrc();
                writer.WriteBytes(EndSentinel);
            }
        }
    }
}
