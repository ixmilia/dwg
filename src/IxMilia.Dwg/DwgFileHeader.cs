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

        public DwgVersionId Version { get; set; }
        public int MaintenenceVersion { get; set; }
        internal int ImagePointer { get; set; }
        public short CodePage { get; set; }

        internal DwgSectionLocator HeaderVariablesLocator { get; set; }
        internal DwgSectionLocator ClassSectionLocator { get; set; }
        internal DwgSectionLocator ObjectMapLocator { get; set; }
        internal DwgSectionLocator UnknownSection1Locator { get; set; }
        internal DwgSectionLocator UnknownSection2Locator { get; set; }
        internal DwgSectionLocator UnknownSection3Locator { get; set; }

        internal DwgFileHeader(DwgVersionId version, int maintenenceVersion, int imagePointer, short codePage)
        {
            Version = version;
            MaintenenceVersion = maintenenceVersion;
            ImagePointer = imagePointer;
            CodePage = codePage;
        }

        internal static DwgFileHeader Parse(BitReader reader)
        {
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
            if (recordLocatorCount >= 1)
            {
                header.HeaderVariablesLocator = DwgSectionLocator.Parse(reader);
            }

            if (recordLocatorCount >= 2)
            {
                header.ClassSectionLocator = DwgSectionLocator.Parse(reader);
            }

            if (recordLocatorCount >= 3)
            {
                header.ObjectMapLocator = DwgSectionLocator.Parse(reader);
            }

            if (recordLocatorCount >= 4)
            {
                // TODO: only if R13C3 or later
                header.UnknownSection1Locator = DwgSectionLocator.Parse(reader);
            }

            if (recordLocatorCount >= 5)
            {
                header.UnknownSection2Locator = DwgSectionLocator.Parse(reader);
            }

            if (recordLocatorCount >= 6)
            {
                header.UnknownSection3Locator = DwgSectionLocator.Parse(reader);
            }

            ushort xorValue;
            switch (recordLocatorCount)
            {
                case 0:
                case 1:
                case 2:
                    xorValue = 0;
                    break;
                case 3:
                    xorValue = 0xAd98;
                    break;
                case 4:
                    xorValue = 0x8101;
                    break;
                case 5:
                    xorValue = 0x3CC4;
                    break;
                case 6:
                    xorValue = 0x8461;
                    break;
                default:
                    throw new DwgReadException("Unsupported record locator count.");
            }

            var crc = (ushort)reader.Read_RS();
            crc = (ushort)(crc ^ xorValue);

            // TODO: compute CRC and compare

            reader.AssertSentinel(HeaderSentinel);

            return header;
        }

        internal struct DwgSectionLocator
        {
            public int RecordNumber { get; set; }
            public int Pointer { get; set; }
            public int Length { get; set; }

            internal DwgSectionLocator(int recordNumber, int pointer, int length)
            {
                RecordNumber = recordNumber;
                Pointer = pointer;
                Length = length;
            }

            internal static DwgSectionLocator Parse(BitReader reader)
            {
                var recordNumber = reader.ReadByte();
                var pointer = reader.ReadInt();
                var length = reader.ReadInt();
                return new DwgSectionLocator(recordNumber, pointer, length);
            }
        }
    }
}
