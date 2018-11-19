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
        public int HandleOrOffset { get; }

        public bool IsEmpty => (int)Code == 0 && HandleOrOffset == 0;

        public DwgHandleReference(DwgHandleReferenceCode code, int offset)
        {
            Code = code;
            HandleOrOffset = offset;
        }

        public DwgHandleReference(int code, int offset)
            : this((DwgHandleReferenceCode)code, offset)
        {
        }

        internal static int ReadSecondHeader(BitReader reader, int byteCount)
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

            return handle;
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

        internal bool IsValidNavigationHandle
        {
            get
            {
                switch (Code)
                {
                    case DwgHandleReferenceCode.HardPointer:
                    case DwgHandleReferenceCode.SoftPointer:
                    case DwgHandleReferenceCode.HandlePlus1:
                    case DwgHandleReferenceCode.HandleMinus1:
                    case DwgHandleReferenceCode.HandlePlusOffset:
                    case DwgHandleReferenceCode.HandleMinusOffset:
                        return true;
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

        internal DwgHandleReference GetNextHandle(DwgHandleReference nextEntityHandle)
        {
            switch (nextEntityHandle.Code)
            {
                case DwgHandleReferenceCode.SoftPointer:
                case DwgHandleReferenceCode.SoftOwner:
                case DwgHandleReferenceCode.HardPointer:
                //case DwgHandleReferenceCode.HardOwner:
                    return nextEntityHandle;
                case DwgHandleReferenceCode.HandlePlus1:
                    return new DwgHandleReference(DwgHandleReferenceCode.HardPointer, HandleOrOffset + 1);
                case DwgHandleReferenceCode.HandleMinus1:
                    return new DwgHandleReference(DwgHandleReferenceCode.HardPointer, HandleOrOffset - 1);
                case DwgHandleReferenceCode.HandlePlusOffset:
                    return new DwgHandleReference(DwgHandleReferenceCode.HardPointer, HandleOrOffset + nextEntityHandle.HandleOrOffset);
                case DwgHandleReferenceCode.HandleMinusOffset:
                    return new DwgHandleReference(DwgHandleReferenceCode.HardPointer, HandleOrOffset - nextEntityHandle.HandleOrOffset);
                default:
                    return default(DwgHandleReference);
            }
        }
    }
}
