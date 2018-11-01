namespace IxMilia.Dwg
{
    internal class DwgUnknownSectionR13C3
    {
        public static void Write(BitWriter writer)
        {
            // unknown section, no sentinels, has a value of `4` at offset 21
            for (int i = 0; i < 20; i++)
            {
                writer.WriteByte(0);
            }

            writer.WriteInt(4);
        }
    }
}
