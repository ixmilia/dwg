using System;
using System.IO;

namespace IxMilia.Dwg
{
    public class DwgFileHeader
    {
        private static byte[] HeaderSentinel = new byte[]
        {
            0x95,
            0xA0,
            0x4E,
            0x28,
            0x99,
            0x82,
            0x1A,
            0xE5,
            0x5E,
            0x41,
            0xE0,
            0x5F,
            0x9D,
            0x3A,
            0x4D,
            0x00
        };

        public static byte[] SecondHeaderStartSentinel = new byte[]
        {
            0xD4, 0x7B, 0x21, 0xCE, 0x28, 0x93, 0x9F, 0xBF, 0x53, 0x24, 0x40, 0x09, 0x12, 0x3C, 0xAA, 0x01
        };

        public static byte[] SecondHeaderEndSentinel = new byte[]
        {
            0x2B, 0x84, 0xDE, 0x31, 0xD7, 0x6C, 0x60, 0x40, 0xAC, 0xDB, 0xBF, 0xF6, 0xED, 0xC3, 0x55, 0xFE
        };

        public static byte[] SecondHeaderMidSentinel = new byte[]
        {
            0x18, 0x78, 0x01
        };

        public DwgVersionId Version { get; set; }
        public int MaintenenceVersion { get; set; }
        internal int ImagePointer { get; set; }
        public short CodePage { get; set; }

        internal DwgSectionLocator HeaderVariablesLocator { get; set; }
        internal DwgSectionLocator ClassSectionLocator { get; set; }
        internal DwgSectionLocator ObjectMapLocator { get; set; }
        internal DwgSectionLocator ObjectFreeSpaceLocator { get; set; }
        internal DwgSectionLocator UnknownSection_PaddingLocator { get; set; }

        private int SecondHeaderPointer => ObjectFreeSpaceLocator.Pointer + ObjectFreeSpaceLocator.Length;

        internal DwgFileHeader(DwgVersionId version, int maintenenceVersion, int imagePointer, short codePage)
        {
            Version = version;
            MaintenenceVersion = maintenenceVersion;
            ImagePointer = imagePointer;
            CodePage = codePage;
        }

        internal static DwgFileHeader Parse(BitReader reader)
        {
            reader.StartCrcCheck();
            var versionString = reader.ReadStringAscii(6);
            var version = DwgVersionIdExtensions.VersionIdFromString(versionString);
            var unknown1 = reader.ReadBytes(6);
            var maintVer = 0;
            if (version == DwgVersionId.R14)
            {
                maintVer = unknown1[5];
            }

            var marker = reader.ReadByte();
            if (marker != 1)
            {
                throw new DwgReadException("Expected value of 1 at offset 13.");
            }

            var imagePointer = reader.ReadInt();
            var unknown2 = reader.ReadBytes(2);
            var codePage = reader.ReadShort();

            var header = new DwgFileHeader(version, maintVer, imagePointer, codePage);

            var recordLocatorCount = reader.ReadInt();
            for (int i = 0; i < recordLocatorCount; i++)
            {
                var locator = DwgSectionLocator.Parse(reader);
                switch (i)
                {
                    case 0:
                        header.HeaderVariablesLocator = locator;
                        break;
                    case 1:
                        header.ClassSectionLocator = locator;
                        break;
                    case 2:
                        header.ObjectMapLocator = locator;
                        break;
                    case 3:
                        header.ObjectFreeSpaceLocator = locator;
                        break;
                    case 4:
                        header.UnknownSection_PaddingLocator = locator;
                        break;
                }
            }

            ushort crcXorValue;
            switch (recordLocatorCount)
            {
                case 0:
                case 1:
                case 2:
                    crcXorValue = 0;
                    break;
                case 3:
                    crcXorValue = 0xAD98;
                    break;
                case 4:
                    crcXorValue = 0x8101;
                    break;
                case 5:
                    crcXorValue = 0x3CC4;
                    break;
                case 6:
                    crcXorValue = 0x8461;
                    break;
                default:
                    throw new DwgReadException("Unsupported record locator count.");
            }

            reader.ValidateCrc(xorValue: crcXorValue);
            reader.ValidateSentinel(HeaderSentinel);

            return header;
        }

        internal void ValidateSecondHeader(BitReader parentReader, DwgHeaderVariables headerVariables)
        {
            if (SecondHeaderPointer == 0)
            {
                return;
            }

            var reader = parentReader.FromOffset(SecondHeaderPointer);
            reader.ValidateSentinel(SecondHeaderStartSentinel);
            var sectionStart = reader.Offset;
            reader.StartCrcCheck();
            var reportedSectionSize = reader.Read_RL();
            var expectedLocation = reader.Read_BL();
            if (expectedLocation != sectionStart - SecondHeaderStartSentinel.Length)
            {
                throw new DwgReadException("Reported second header location incorrect.");
            }

            var version = DwgVersionIdExtensions.VersionIdFromString(reader.ReadStringAscii(6));
            if (version != Version)
            {
                throw new DwgReadException("Inconsistent reported version.");
            }

            reader.ReadBytes(6);
            reader.Read_B();
            reader.Read_B();
            reader.Read_B();
            reader.Read_B();
            reader.ReadBytes(2);
            reader.ValidateBytes(SecondHeaderMidSentinel);

            var recordLocatorCount = reader.ReadByte();
            for (int i = 0; i < recordLocatorCount; i++)
            {
                var id = reader.ReadByte();
                var pointer = reader.Read_BL();
                var length = reader.Read_BL();

                if (pointer != 0)
                {
                    switch (i)
                    {
                        case 0:
                            HeaderVariablesLocator.ValidateLocator(id, pointer, length);
                            break;
                        case 1:
                            ClassSectionLocator.ValidateLocator(id, pointer, length);
                            break;
                        case 2:
                            ObjectMapLocator.ValidateLocator(id, pointer, length);
                            break;
                        case 3:
                            ObjectFreeSpaceLocator.ValidateLocator(id, pointer, length);
                            break;
                        case 4:
                            UnknownSection_PaddingLocator.ValidateLocator(id, pointer, length);
                            break;
                    }
                }
            }

            var handleRecordCount = reader.Read_BS();
            for (int i = 0; i < handleRecordCount; i++)
            {
                var byteCount = reader.Read_RC();
                var id = reader.Read_RC();
                var handle = DwgHandleReference.ReadSecondHeader(reader, byteCount);

                if (byteCount > 0)
                {
                    if (id != i)
                    {
                        throw new DwgReadException("Invalid record handle ID.");
                    }

                    var actualHandle = new DwgHandle?();
                    switch (i)
                    {
                        case 0:
                            actualHandle = headerVariables.NextAvailableHandle.AsAbsoluteHandle();
                            break;
                        case 1:
                            actualHandle = headerVariables.BlockControlObjectHandle.AsAbsoluteHandle();
                            break;
                        case 2:
                            actualHandle = headerVariables.LayerControlObjectHandle.AsAbsoluteHandle();
                            break;
                        case 3:
                            actualHandle = headerVariables.StyleObjectControlHandle.AsAbsoluteHandle();
                            break;
                        case 4:
                            actualHandle = headerVariables.LineTypeObjectControlHandle.AsAbsoluteHandle();
                            break;
                        case 5:
                            actualHandle = headerVariables.ViewControlObjectHandle.AsAbsoluteHandle();
                            break;
                        case 6:
                            actualHandle = headerVariables.UcsControlObjectHandle.AsAbsoluteHandle();
                            break;
                        case 7:
                            actualHandle = headerVariables.ViewPortControlObjectHandle.AsAbsoluteHandle();
                            break;
                        case 8:
                            actualHandle = headerVariables.AppIdControlObjectHandle.AsAbsoluteHandle();
                            break;
                        case 9:
                            actualHandle = headerVariables.DimStyleControlObjectHandle.AsAbsoluteHandle();
                            break;
                        case 10:
                            actualHandle = headerVariables.ViewPortEntityHeaderControlObjectHandle.AsAbsoluteHandle();
                            break;
                        case 11:
                            actualHandle = headerVariables.NamedObjectsDictionaryHandle.AsAbsoluteHandle();
                            break;
                        case 12:
                            actualHandle = headerVariables.MLineStyleDictionaryHandle.AsAbsoluteHandle();
                            break;
                        case 13:
                            actualHandle = headerVariables.GroupDictionaryHandle.AsAbsoluteHandle();
                            break;
                    }

                    if (actualHandle.HasValue)
                    {
                        if (handle != actualHandle.GetValueOrDefault())
                        {
                            throw new DwgReadException($"Invalid record handle ID at location {i}.  Expected: {handle}, Actual: {actualHandle}");
                        }
                    }
                }
            }

            reader.ReadByte();
            reader.ValidateCrc(initialValue: DwgHeaderVariables.InitialCrcValue);
            var junkByteCount = Math.Max(0, reportedSectionSize - (reader.Offset - sectionStart));
            reader.SkipBytes(junkByteCount);
            reader.ValidateSentinel(SecondHeaderEndSentinel);

            var computedSectionSize = reader.Offset - sectionStart - SecondHeaderEndSentinel.Length;
            if (computedSectionSize != reportedSectionSize)
            {
                throw new DwgReadException($"Reported and actual second header sizes differ.  Expected: {reportedSectionSize}, Actual: {computedSectionSize}");
            }
        }

        internal void Write(BitWriter writer)
        {
            writer.StartCrcCalculation();
            writer.WriteStringAscii(Version.VersionString(), nullTerminated: false);
            writer.WriteBytes(0, 0, 0, 0, 0);
            if (Version == DwgVersionId.R14)
            {
                writer.WriteByte((byte)MaintenenceVersion);
            }
            else
            {
                writer.WriteByte(0);
            }

            writer.WriteByte(1);
            writer.WriteInt(ImagePointer);
            writer.WriteBytes(0, 0);
            writer.WriteShort(CodePage);

            writer.WriteInt(5); // 5 records
            HeaderVariablesLocator.Write(writer);
            ClassSectionLocator.Write(writer);
            ObjectMapLocator.Write(writer);
            ObjectFreeSpaceLocator.Write(writer);
            UnknownSection_PaddingLocator.Write(writer);

            writer.WriteCrc(xorValue: 0x3CC4); // value for 5 records
            writer.WriteBytes(HeaderSentinel);
        }

        internal void WriteSecondHeader(BitWriter writer, DwgHeaderVariables headerVariables, int pointer)
        {
            // write to memory the backtrack to fill in size and crc
            using (var ms = new MemoryStream())
            {
                var tempWriter = new BitWriter(ms);
                tempWriter.WriteBytes(SecondHeaderStartSentinel);

                var sizeOffset = tempWriter.Position;
                tempWriter.Write_RL(0); // size, filled in later
                tempWriter.Write_BL(pointer);
                tempWriter.WriteStringAscii(Version.VersionString(), nullTerminated: false);
                tempWriter.WriteBytes(new byte[] { 0, 0, 0, 0, 0, 0 }); // 6 zero bytes
                tempWriter.WriteBits(0x00000000, 4); // 4 zero bits
                tempWriter.WriteBytes(new byte[] { 0, 0 }); // 2 unknown bytes
                tempWriter.WriteBytes(SecondHeaderMidSentinel);

                tempWriter.WriteByte(5); // record locator count
                HeaderVariablesLocator.Write(tempWriter, writingSecondHeader: true);
                ClassSectionLocator.Write(tempWriter, writingSecondHeader: true);
                ObjectMapLocator.Write(tempWriter, writingSecondHeader: true);
                ObjectFreeSpaceLocator.Write(tempWriter, writingSecondHeader: true);
                UnknownSection_PaddingLocator.Write(tempWriter, writingSecondHeader: true);

                tempWriter.Write_BS(14);
                headerVariables.NextAvailableHandle.WriteSecondHeader(tempWriter, 0);
                headerVariables.BlockControlObjectHandle.WriteSecondHeader(tempWriter, 1);
                headerVariables.LayerControlObjectHandle.WriteSecondHeader(tempWriter, 2);
                headerVariables.StyleObjectControlHandle.WriteSecondHeader(tempWriter, 3);
                headerVariables.LineTypeObjectControlHandle.WriteSecondHeader(tempWriter, 4);
                headerVariables.ViewControlObjectHandle.WriteSecondHeader(tempWriter, 5);
                headerVariables.UcsControlObjectHandle.WriteSecondHeader(tempWriter, 6);
                headerVariables.ViewPortControlObjectHandle.WriteSecondHeader(tempWriter, 7);
                headerVariables.AppIdControlObjectHandle.WriteSecondHeader(tempWriter, 8);
                headerVariables.DimStyleControlObjectHandle.WriteSecondHeader(tempWriter, 9);
                headerVariables.ViewPortEntityHeaderControlObjectHandle.WriteSecondHeader(tempWriter, 10);
                headerVariables.NamedObjectsDictionaryHandle.WriteSecondHeader(tempWriter, 11);
                headerVariables.MLineStyleDictionaryHandle.WriteSecondHeader(tempWriter, 12);
                headerVariables.GroupDictionaryHandle.WriteSecondHeader(tempWriter, 13);

                tempWriter.WriteByte(0); // unknown
                tempWriter.AlignByte();
                var crcOffset = tempWriter.Position;
                tempWriter.WriteShort(0); // CRC, filled in later

                if (Version == DwgVersionId.R14)
                {
                    // unknown garbage bytes
                    tempWriter.WriteBytes(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
                }

                tempWriter.WriteBytes(SecondHeaderEndSentinel);

                // get bytes of header
                var secondHeaderBytes = tempWriter.AsBytes();

                // fill in size
                var sizeBytes = BitConverter.GetBytes(secondHeaderBytes.Length - SecondHeaderStartSentinel.Length - SecondHeaderEndSentinel.Length);
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(sizeBytes);
                }

                Array.Copy(sizeBytes, 0, secondHeaderBytes, sizeOffset, sizeBytes.Length);

                // fill in crc
                var crc = BitReaderExtensions.ComputeCRC(secondHeaderBytes, sizeOffset, crcOffset - sizeOffset, DwgHeaderVariables.InitialCrcValue);
                var crcBytes = BitConverter.GetBytes(crc);
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(crcBytes);
                }

                Array.Copy(crcBytes, 0, secondHeaderBytes, crcOffset, crcBytes.Length);

                // write to the real writer
                writer.WriteBytes(secondHeaderBytes);
            }
        }

        internal struct DwgSectionLocator
        {
            private const byte HeaderVariablesRecordNumber = 0;
            private const byte ClassSectionLocatorRecordNumber = 1;
            private const byte ObjectMapLocatorRecordNumber = 2;
            private const byte ObjectFreeSpaceLocatorRecordNumber = 3;
            private const byte UnknownSection_PaddingLocatorRecordNumber = 4;

            public byte RecordNumber { get; }
            public int Pointer { get; }
            public int Length { get; }

            public bool IsDefault => RecordNumber == default(int) && Pointer == default(int) && Length == default(int);

            private DwgSectionLocator(byte recordNumber, int pointer, int length)
            {
                RecordNumber = recordNumber;
                Pointer = pointer;
                Length = length;
            }

            public void Write(BitWriter writer, bool writingSecondHeader = false)
            {
                writer.WriteByte(RecordNumber);
                if (writingSecondHeader)
                {
                    writer.Write_BL(Pointer);
                    writer.Write_BL(Length);
                }
                else
                {
                    writer.WriteInt(Pointer);
                    writer.WriteInt(Length);
                }
            }

            public void ValidateLocator(byte recordNumber, int pointer, int length)
            {
                if (recordNumber != RecordNumber)
                {
                    throw new DwgReadException($"Invalid record number.  Expected: {RecordNumber}, Actual: {recordNumber}");
                }

                if (pointer != Pointer)
                {
                    throw new DwgReadException($"Invalid pointer.  Expected: {Pointer}, Actual: {pointer}");
                }

                if (length != Length)
                {
                    throw new DwgReadException($"Invalid length.  Expected: {Length}, Actual: {length}");
                }
            }

            public static DwgSectionLocator Parse(BitReader reader)
            {
                var recordNumber = reader.ReadByte();
                var pointer = reader.ReadInt();
                var length = reader.ReadInt();
                return new DwgSectionLocator(recordNumber, pointer, length);
            }

            public static DwgSectionLocator HeaderVariablesLocator(int pointer, int length)
            {
                return new DwgSectionLocator(HeaderVariablesRecordNumber, pointer, length);
            }

            public static DwgSectionLocator ClassSectionLocator(int pointer, int length)
            {
                return new DwgSectionLocator(ClassSectionLocatorRecordNumber, pointer, length);
            }

            public static DwgSectionLocator ObjectMapLocator(int pointer, int length)
            {
                return new DwgSectionLocator(ObjectMapLocatorRecordNumber, pointer, length);
            }

            public static DwgSectionLocator ObjectFreeSpaceLocator(int pointer, int length)
            {
                return new DwgSectionLocator(ObjectFreeSpaceLocatorRecordNumber, pointer, length);
            }

            public static DwgSectionLocator UnknownSection_PaddingLocator(int pointer, int length)
            {
                return new DwgSectionLocator(UnknownSection_PaddingLocatorRecordNumber, pointer, length);
            }
        }
    }
}
