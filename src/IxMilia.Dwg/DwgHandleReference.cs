using System;

namespace IxMilia.Dwg
{
    public enum DwgHandleReferenceCode : byte
    {
        Declaration = 0x00,
        None = 0x02,
        SoftPointer = 0x03,
        HardPointer = 0x04,
        SoftOwner = 0x05,
        HardOwner = 0x06,
        HandlePlus1 = 0x06,
        HandleMinus1 = 0x08,
        HandlePlusOffset = 0x0A,
        HandleMinusOffset = 0x0C,
    }

    public struct DwgHandleReference : IEquatable<DwgHandleReference>
    {
        public DwgHandleReferenceCode Code { get; }
        public int HandleOrOffset { get; }

        public bool IsEmpty => (int)Code == 0 && HandleOrOffset == 0;

        public DwgHandleReference(DwgHandleReferenceCode code, int offset)
        {
            Code = code;
            HandleOrOffset = offset;
        }

        internal void WriteSecondHeader(BitWriter writer, int id)
        {
            writer.Write_RC((byte)HandleOrOffset);
            writer.Write_RC(1); // byte count
            writer.Write_RC((byte)id);
        }

        public static bool operator ==(DwgHandleReference r1, DwgHandleReference r2)
        {
            return r1.Code == r2.Code && r1.HandleOrOffset == r2.HandleOrOffset;
        }

        public static bool operator !=(DwgHandleReference r1, DwgHandleReference r2)
        {
            return !(r1 == r2);
        }

        public override bool Equals(object obj)
        {
            return obj is DwgHandleReference && Equals((DwgHandleReference)obj);
        }

        public bool Equals(DwgHandleReference other)
        {
            return Code == other.Code &&
                   HandleOrOffset == other.HandleOrOffset;
        }

        public override int GetHashCode()
        {
            var hashCode = -1538022967;
            hashCode = hashCode * -1521134295 + Code.GetHashCode();
            hashCode = hashCode * -1521134295 + HandleOrOffset.GetHashCode();
            return hashCode;
        }
    }
}
