using System;

namespace IxMilia.Dwg
{
    public struct DwgHandle : IComparable
    {
        private uint _rawValue;

        public bool IsNull => _rawValue == 0;

        public DwgHandle(uint rawValue)
        {
            _rawValue = rawValue;
        }

        public static explicit operator uint(DwgHandle handle) => handle._rawValue;

        public DwgHandleReference MakeDeclarationHandleReference()
        {
            return new DwgHandleReference(DwgHandleReferenceCode.Declaration, _rawValue);
        }

        public DwgHandleReference MakeHandleReference(DwgHandleReferenceCode code)
        {
            switch (code)
            {
                case DwgHandleReferenceCode.Declaration:
                case DwgHandleReferenceCode.SoftPointer:
                case DwgHandleReferenceCode.SoftOwner:
                case DwgHandleReferenceCode.HardPointer:
                case DwgHandleReferenceCode.HardOwner:
                case DwgHandleReferenceCode.None:
                    return new DwgHandleReference(code, _rawValue);
                default:
                    throw new InvalidOperationException($"Unexpected handle reference code {code}.");
            }
        }

        public DwgHandleReference MakeNavigationReference(DwgHandle destinationHandle, DwgHandleReferenceCode handleReferenceCode)
        {
            if (destinationHandle.IsNull)
            {
                return destinationHandle.MakeHandleReference(handleReferenceCode);
            }

            var handleDistance = (int)destinationHandle._rawValue - _rawValue;
            switch (handleDistance)
            {
                case 1:
                    return new DwgHandleReference(DwgHandleReferenceCode.HandlePlus1, 0);
                case -1:
                    return new DwgHandleReference(DwgHandleReferenceCode.HandleMinus1, 0);
                default:
                    return destinationHandle.MakeHandleReference(handleReferenceCode);
            }
        }

        public DwgHandle ResolveHandleReference(DwgHandleReference handleReference)
        {
            switch (handleReference.Code)
            {
                case DwgHandleReferenceCode.Declaration:
                case DwgHandleReferenceCode.SoftPointer:
                case DwgHandleReferenceCode.SoftOwner:
                case DwgHandleReferenceCode.HardPointer:
                case DwgHandleReferenceCode.None:
                    //case DwgHandleReferenceCode.HardOwner:
                    return new DwgHandle(handleReference.HandleOrOffset);
                case DwgHandleReferenceCode.HandlePlus1:
                    return new DwgHandle(_rawValue + 1);
                case DwgHandleReferenceCode.HandleMinus1:
                    return new DwgHandle(_rawValue - 1);
                case DwgHandleReferenceCode.HandlePlusOffset:
                    return new DwgHandle(_rawValue + handleReference.HandleOrOffset);
                case DwgHandleReferenceCode.HandleMinusOffset:
                    return new DwgHandle(_rawValue - handleReference.HandleOrOffset);
                default:
                    throw new InvalidOperationException($"Unexpected resolution on handle type {handleReference.Code}.");
            }
        }

        public static bool operator ==(DwgHandle h1, DwgHandle h2)
        {
            return h1._rawValue == h2._rawValue;
        }

        public static bool operator !=(DwgHandle h1, DwgHandle h2)
        {
            return h1._rawValue != h2._rawValue;
        }

        public override bool Equals(object obj)
        {
            return obj is DwgHandle handle && _rawValue == handle._rawValue;
        }

        public override int GetHashCode()
        {
            return _rawValue.GetHashCode();
        }

        public override string ToString()
        {
            return $"{_rawValue:X}";
        }

        public int CompareTo(object obj)
        {
            return _rawValue.CompareTo(((DwgHandle)obj)._rawValue);
        }
    }
}
