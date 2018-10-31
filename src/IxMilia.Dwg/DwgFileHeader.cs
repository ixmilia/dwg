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
        internal DwgSectionLocator UnknownSection_R13C3AndLaterLocator { get; set; }
        internal DwgSectionLocator UnknownSection_PaddingLocator { get; set; }

        private int SecondHeaderPointer => UnknownSection_R13C3AndLaterLocator.Pointer + UnknownSection_R13C3AndLaterLocator.Length;

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
                        header.UnknownSection_R13C3AndLaterLocator = locator;
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
            var reader = parentReader.FromOffset(SecondHeaderPointer);
            var sectionStart = reader.Offset;
            reader.ValidateSentinel(SecondHeaderStartSentinel);
            reader.StartCrcCheck();
            var reportedSectionSize = reader.Read_RL();
            var expectedLocation = reader.Read_BL();
            if (expectedLocation != sectionStart)
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
                            UnknownSection_R13C3AndLaterLocator.ValidateLocator(id, pointer, length);
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
                var handle = reader.Read_RC();
                var byteCount = reader.Read_RC();
                var id = reader.Read_RC();

                if (byteCount > 0)
                {
                    if (id != i)
                    {
                        throw new DwgReadException("Invalid record handle ID.");
                    }

                    var actualHandle = -1;
                    switch (i)
                    {
                        case 0:
                            actualHandle = headerVariables.ModelSpaceBlockRecordHandle.HandleOrOffset;
                            break;
                        case 1:
                            // unknown
                            break;
                        case 2:
                            actualHandle = headerVariables.BlockControlObjectHandle.HandleOrOffset;
                            break;
                        case 3:
                            actualHandle = headerVariables.LayerControlObjectHandle.HandleOrOffset;
                            break;
                        case 4:
                            actualHandle = headerVariables.StyleObjectControlHandle.HandleOrOffset;
                            break;
                        case 5:
                            actualHandle = headerVariables.LineTypeObjectControlHandle.HandleOrOffset;
                            break;
                        case 6:
                            actualHandle = headerVariables.ViewControlObjectHandle.HandleOrOffset;
                            break;
                        case 7:
                            actualHandle = headerVariables.UcsControlObjectHandle.HandleOrOffset;
                            break;
                        case 8:
                            actualHandle = headerVariables.ViewPortControlObjectHandle.HandleOrOffset;
                            break;
                        case 9:
                            actualHandle = headerVariables.AppIdControlObjectHandle.HandleOrOffset;
                            break;
                        case 10:
                            actualHandle = headerVariables.DimStyleControlObjectHandle.HandleOrOffset;
                            break;
                        case 11:
                            actualHandle = headerVariables.ViewPortEntityHeaderControlObjectHandle.HandleOrOffset;
                            break;
                        case 12:
                            actualHandle = headerVariables.NamedObjectsDictionaryHandle.HandleOrOffset;
                            break;
                        case 13:
                            actualHandle = headerVariables.MLineStyleDictionaryHandle.HandleOrOffset;
                            break;
                    }

                    if (actualHandle > 0)
                    {
                        if (handle != actualHandle)
                        {
                            throw new DwgReadException($"Invalid record handle ID at location {i}.  Expected: {handle}, Actual: {actualHandle}");
                        }
                    }
                }
            }

            reader.ReadByte();
            reader.ValidateCrc(initialValue: DwgHeaderVariables.InitialCrcValue);
            if (version == DwgVersionId.R14)
            {
                reader.ReadBytes(8);
            }

            reader.ValidateSentinel(SecondHeaderEndSentinel);

            var computedSectionSize = reader.Offset - sectionStart - SecondHeaderStartSentinel.Length - SecondHeaderEndSentinel.Length;
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
            UnknownSection_R13C3AndLaterLocator.Write(writer);
            UnknownSection_PaddingLocator.Write(writer);

            writer.WriteCrc(xorValue: 0x3CC4); // value for 5 records
            writer.WriteBytes(HeaderSentinel);
        }

        internal void WriteSecondHeader(BitWriter writer, DwgHeaderVariables headerVariables, int pointer, out int sizeOffset, out int crcOffset)
        {
            writer.WriteBytes(SecondHeaderStartSentinel);

            sizeOffset = (int)writer.BaseStream.Position;
            writer.Write_RL(0); // size, filled in later
            writer.Write_BL(pointer);
            writer.WriteStringAscii(Version.VersionString(), nullTerminated: false);
            writer.WriteBytes(new byte[] { 0, 0, 0, 0, 0, 0 }); // 6 zero bytes
            writer.WriteBits(0x00000000, 4); // 4 zero bits
            writer.WriteBytes(new byte[] { 0, 0 }); // 2 unknown bytes
            writer.WriteBytes(SecondHeaderMidSentinel);

            writer.WriteByte(5); // record locator count
            HeaderVariablesLocator.Write(writer, writingSecondHeader: true);
            ClassSectionLocator.Write(writer, writingSecondHeader: true);
            ObjectMapLocator.Write(writer, writingSecondHeader: true);
            UnknownSection_R13C3AndLaterLocator.Write(writer, writingSecondHeader: true);
            UnknownSection_PaddingLocator.Write(writer, writingSecondHeader: true);

            writer.Write_BS(14);
            headerVariables.ModelSpaceBlockRecordHandle.WriteSecondHeader(writer, 0);
            new DwgHandleReference().WriteSecondHeader(writer, 1); // TODO: unknown
            headerVariables.BlockControlObjectHandle.WriteSecondHeader(writer, 2);
            headerVariables.LayerControlObjectHandle.WriteSecondHeader(writer, 3);
            headerVariables.StyleObjectControlHandle.WriteSecondHeader(writer, 4);
            headerVariables.LineTypeObjectControlHandle.WriteSecondHeader(writer, 5);
            headerVariables.ViewControlObjectHandle.WriteSecondHeader(writer, 6);
            headerVariables.UcsControlObjectHandle.WriteSecondHeader(writer, 7);
            headerVariables.ViewPortControlObjectHandle.WriteSecondHeader(writer, 8);
            headerVariables.AppIdControlObjectHandle.WriteSecondHeader(writer, 9);
            headerVariables.DimStyleControlObjectHandle.WriteSecondHeader(writer, 10);
            headerVariables.ViewPortControlObjectHandle.WriteSecondHeader(writer, 11);
            headerVariables.NamedObjectsDictionaryHandle.WriteSecondHeader(writer, 12);
            headerVariables.MLineStyleDictionaryHandle.WriteSecondHeader(writer, 13);

            writer.WriteByte(0); // unknown
            writer.AlignByte();
            crcOffset = (int)writer.BaseStream.Position;
            writer.WriteShort(0); // CRC, filled in later

            if (Version == DwgVersionId.R14)
            {
                // unknown garbage bytes
                writer.WriteBytes(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
            }

            writer.WriteBytes(SecondHeaderEndSentinel);
        }

        internal struct DwgSectionLocator
        {
            private const byte HeaderVariablesRecordNumber = 0;
            private const byte ClassSectionLocatorRecordNumber = 1;
            private const byte ObjectMapLocatorRecordNumber = 2;
            private const byte UnknownSection_R13C3AndLaterLocatorRecordNumber = 3;
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

            public static DwgSectionLocator UnknownSection_R13C3AndLaterLocator(int pointer, int length)
            {
                return new DwgSectionLocator(UnknownSection_R13C3AndLaterLocatorRecordNumber, pointer, length);
            }

            public static DwgSectionLocator UnknownSection_PaddingLocator(int pointer, int length)
            {
                return new DwgSectionLocator(UnknownSection_PaddingLocatorRecordNumber, pointer, length);
            }
        }
    }
}
