using System;
using System.Collections.Generic;

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
        internal uint HandleOrOffset { get; }

        public bool IsEmpty => (int)Code == 0 && HandleOrOffset == 0;

        public DwgHandleReference(DwgHandleReferenceCode code, uint offset)
        {
            Code = code;
            HandleOrOffset = offset;
        }

        public DwgHandleReference(int code, uint offset)
            : this((DwgHandleReferenceCode)code, offset)
        {
        }

        internal static DwgHandle ReadSecondHeader(BitReader reader, int byteCount)
        {
            var handleBytes = reader.ReadBytes(byteCount);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(handleBytes);
            }

            var handle = 0;
            foreach (var b in handleBytes)
            {
                handle = (handle << 8) + b;
            }

            return new DwgHandle((uint)handle);
        }

        internal void WriteSecondHeader(BitWriter writer, int id)
        {
            // compute the minimum number of bytes necessary to encode the handle
            var handleBytes = BitConverter.GetBytes(HandleOrOffset);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(handleBytes);
            }

            var bytesList = new List<byte>(handleBytes);
            while (bytesList.Count > 0 && bytesList[0] == 0)
            {
                bytesList.RemoveAt(0);
            }

            if (bytesList.Count == 0)
            {
                // ensure there's a least one byte
                bytesList.Add(0);
            }

            writer.Write_RC((byte)bytesList.Count);
            writer.Write_RC((byte)id);
            writer.WriteBytes(bytesList.ToArray());
        }

        internal bool PointsToNull
        {
            get
            {
                switch (Code)
                {
                    case DwgHandleReferenceCode.SoftOwner:
                    case DwgHandleReferenceCode.SoftPointer:
                    case DwgHandleReferenceCode.HardOwner:
                    case DwgHandleReferenceCode.HardPointer:
                        return HandleOrOffset == 0;
                    default:
                        return false;
                }
            }
        }

        internal bool IsNullNavigationHandle => Code == DwgHandleReferenceCode.HardPointer && HandleOrOffset == 0;

        internal bool IsValidNavigationHandle
        {
            get
            {
                switch (Code)
                {
                    case DwgHandleReferenceCode.HandlePlus1:
                    case DwgHandleReferenceCode.HandleMinus1:
                    case DwgHandleReferenceCode.HandlePlusOffset:
                    case DwgHandleReferenceCode.HandleMinusOffset:
                        return true;
                    case DwgHandleReferenceCode.HardPointer:
                    case DwgHandleReferenceCode.SoftPointer:
                        return HandleOrOffset != 0;
                    default:
                        return false;
                }
            }
        }

        public override string ToString()
        {
            return $"{(int)Code}.{HandleOrOffset:X}";
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

        public DwgHandle AsDeclarationHandle()
        {
            switch (Code)
            {
                case DwgHandleReferenceCode.Declaration:
                    return new DwgHandle(HandleOrOffset);
                default:
                    throw new InvalidOperationException($"Handle reference of type {Code} cannot be used as a declaration handle.");
            }
        }

        public DwgHandle AsAbsoluteHandle()
        {
            switch (Code)
            {
                case DwgHandleReferenceCode.Declaration:
                case DwgHandleReferenceCode.SoftPointer:
                case DwgHandleReferenceCode.SoftOwner:
                case DwgHandleReferenceCode.HardPointer:
                case DwgHandleReferenceCode.HardOwner:
                    return new DwgHandle(HandleOrOffset);
                default:
                    throw new InvalidOperationException($"Handle reference of type {Code} cannot be used as an absolute handle.");
            }
        }
    }
}
