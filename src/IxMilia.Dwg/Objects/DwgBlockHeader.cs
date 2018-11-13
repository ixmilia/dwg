using System;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgBlockHeader : DwgObject
    {
        public DwgBlock Block { get; set; }

        public DwgBlockHeader(string name)
            : this()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Name cannot be null.");
            }

            Name = name;
        }

        internal override DwgHandleReferenceCode ExpectedNullHandleCode => DwgHandleReferenceCode.SoftOwner;

        internal override void PoseParse(BitReader reader, DwgObjectCache objectCache)
        {
            if (BlockControlHandle.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect block header control object parent handle code.");
            }

            if (BlockEntityHandle.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect block entity handle code.");
            }

            if (!IsXRef && !IsOverlaidXref)
            {
                if (_firstEntityHandle.Code != DwgHandleReferenceCode.HardPointer)
                {
                    throw new DwgReadException("Incorrect first entity handle code.");
                }

                if (_lastEntityHandle.Code != DwgHandleReferenceCode.HardPointer)
                {
                    throw new DwgReadException("Incorrect last entity handle code.");
                }
            }

            if (EndBlockEntityHandle.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect end block entity handle code.");
            }

            Block = objectCache.GetObject<DwgBlock>(reader, BlockEntityHandle.HandleOrOffset);
        }
    }
}
