namespace IxMilia.Dwg
{
    public class DwgClassDefinition
    {
        private const short ProducesEntitiesConstant = 0x1F2;
        private const short ProducedObjectsConstant = 0x1F3;

        public short Number { get; private set; }
        public short VersionFlag { get; private set; }
        public string ApplicationName { get; set; }
        public string CPlusPlusClassName { get; set; }
        public string DxfClassName { get; set; }
        public bool WasAZombie { get; set; }
        public bool ProducesEntities { get; set; }

        public bool ProducesObjects
        {
            get => !ProducesEntities;
            set => ProducesEntities = !value;
        }

        private DwgClassDefinition()
        {
        }

        internal static DwgClassDefinition Parse(BitReader reader)
        {
            var def = new DwgClassDefinition();
            def.Number = reader.Read_BS();
            def.VersionFlag = reader.Read_BS();
            def.ApplicationName = reader.Read_T();
            def.CPlusPlusClassName = reader.Read_T();
            def.DxfClassName = reader.Read_T();
            def.WasAZombie = reader.Read_B();
            def.ProducesEntities = reader.Read_BS() == ProducesEntitiesConstant;
            return def;
        }

        internal void Write(BitWriter writer)
        {
            writer.Write_BS(Number);
            writer.Write_BS(VersionFlag);
            writer.Write_T(ApplicationName);
            writer.Write_T(CPlusPlusClassName);
            writer.Write_T(DxfClassName);
            writer.Write_B(WasAZombie);
            writer.Write_BS(ProducesEntities ? ProducesEntitiesConstant : ProducedObjectsConstant);
        }
    }
}
