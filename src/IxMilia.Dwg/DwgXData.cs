namespace IxMilia.Dwg
{
    public class DwgXData
    {
        internal static DwgXData Parse(BitReader reader)
        {
            // TODO: do something meaningful
            var xdataLength = reader.Read_BS();
            while (xdataLength > 0)
            {
                var applicationHandle = reader.Read_H();
                var rawData = reader.ReadBytes(xdataLength);
                xdataLength = reader.Read_BS();
            }

            return new DwgXData();
        }

        internal void Write(BitWriter writer)
        {
            // TODO: do something meaningful
            writer.Write_BS(0);
        }
    }
}
