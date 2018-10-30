using System.Collections.Generic;

namespace IxMilia.Dwg
{
    internal class DwgObjectMap
    {
        public IDictionary<int, int> HandleOffsets = new Dictionary<int, int>();

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
                    objectMap.HandleOffsets[handle] = location;
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

        public void Write(BitWriter writer)
        {
            writer.StartCrcCalculation(initialValue: DwgHeaderVariables.InitialCrcValue);
            writer.WriteShortBigEndian(2); // size
            // TODO: write objects
            writer.WriteCrc(writeCrcAsMsb: true);
        }
    }
}
