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

        internal DwgClassDefinition(short number, short versionFlag, string applicationName, string cPlusPlusClassName, string dxfClassName, bool wasAZombie, bool producesEntities)
        {
            Number = number;
            VersionFlag = versionFlag;
            ApplicationName = applicationName;
            CPlusPlusClassName = cPlusPlusClassName;
            DxfClassName = dxfClassName;
            WasAZombie = wasAZombie;
            ProducesEntities = producesEntities;
        }

        internal static DwgClassDefinition Parse(BitReader reader)
        {
            var number = reader.Read_BS();
            var versionFlag = reader.Read_BS();
            var applicationName = reader.Read_T();
            var cPlusPlusClassName = reader.Read_T();
            var dxfClassName = reader.Read_T();
            var wasAZombie = reader.Read_B();
            var producesEntities = reader.Read_BS() == ProducesEntitiesConstant;
            var def = new DwgClassDefinition(number, versionFlag, applicationName, cPlusPlusClassName, dxfClassName, wasAZombie, producesEntities);
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
