#nullable enable

using System;
using System.Collections.Generic;

namespace IxMilia.Dwg
{
    public class DwgClassDefinition : IEquatable<DwgClassDefinition>
    {
        private const short ProducesEntitiesConstant = 0x1F2;
        private const short ProducedObjectsConstant = 0x1F3;

        public short Number { get; internal set; }
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

        public override string ToString()
        {
            return $"{ApplicationName}|{CPlusPlusClassName}|{DxfClassName}";
        }

        public static bool operator ==(DwgClassDefinition? left, DwgClassDefinition? right)
        {
            if (left is null && right is null)
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            if (ReferenceEquals(left, right))
            {
                return true;
            }

            return left.Number == right.Number
                && left.VersionFlag == right.VersionFlag
                && left.ApplicationName == right.ApplicationName
                && left.CPlusPlusClassName == right.CPlusPlusClassName
                && left.DxfClassName == right.DxfClassName
                && left.WasAZombie == right.WasAZombie
                && left.ProducesEntities == right.ProducesEntities;
        }

        public static bool operator !=(DwgClassDefinition left, DwgClassDefinition right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            if (obj is DwgClassDefinition classDef)
            {
                return Equals(classDef);
            }

            return false;
        }

        public bool Equals(DwgClassDefinition? other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            int hashCode = 1130590086;
            hashCode = hashCode * -1521134295 + Number.GetHashCode();
            hashCode = hashCode * -1521134295 + VersionFlag.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ApplicationName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CPlusPlusClassName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DxfClassName);
            hashCode = hashCode * -1521134295 + WasAZombie.GetHashCode();
            hashCode = hashCode * -1521134295 + ProducesEntities.GetHashCode();
            hashCode = hashCode * -1521134295 + ProducesObjects.GetHashCode();
            return hashCode;
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
