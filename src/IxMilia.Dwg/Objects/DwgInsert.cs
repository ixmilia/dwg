using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgInsert
    {
        public DwgBlockHeader BlockHeader { get; set; }
        public DwgSeqEnd SeqEnd { get; private set; } = new DwgSeqEnd();
        public List<DwgAttribute> Attributes { get; } = new List<DwgAttribute>();

        internal DwgHandleReference _firstAttribHandle;
        internal DwgHandleReference _lastAttribHandle;
        internal DwgHandleReference _blockHeaderHandle;
        internal DwgHandleReference _seqEndHandle;

        internal override IEnumerable<DwgObject> ChildItems
        {
            get
            {
                yield return BlockHeader;
                foreach (var attrib in Attributes)
                {
                    yield return attrib;
                }

                yield return SeqEnd;
            }
        }

        public DwgInsert(DwgBlockHeader blockHeader, IEnumerable<DwgAttribute> attributes)
            : this()
        {
            BlockHeader = blockHeader;
            Attributes.AddRange(attributes);
        }

        public DwgInsert(DwgBlockHeader blockHeader, params DwgAttribute[] attributes)
            : this(blockHeader, (IEnumerable<DwgAttribute>)attributes)
        {
        }

        internal override void ReadPostData(BitReader reader)
        {
            _blockHeaderHandle = reader.Read_H();
            if (_blockHeaderHandle.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect block header handle code.");
            }

            if (_hasAttributes)
            {
                _firstAttribHandle = reader.Read_H();
                if (_firstAttribHandle.Code != DwgHandleReferenceCode.HardPointer)
                {
                    throw new DwgReadException("Incorrect attribute handle code.");
                }

                _lastAttribHandle = reader.Read_H();
                if (_lastAttribHandle.Code != DwgHandleReferenceCode.HardPointer)
                {
                    throw new DwgReadException("Incorrect attribute handle code.");
                }

                _seqEndHandle = reader.Read_H();
                if (_seqEndHandle.Code != DwgHandleReferenceCode.SoftPointer)
                {
                    throw new DwgReadException("Incorrect seqend handle code.");
                }
            }
        }

        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache)
        {
            BlockHeader = objectCache.GetObject<DwgBlockHeader>(reader, _blockHeaderHandle.HandleOrOffset);
            Attributes.Clear();
            if (_hasAttributes)
            {
                var attributes = DwgEntityHelpers.EntitiesFromHandlePointer<DwgAttribute>(objectCache, reader, _firstAttribHandle);
                Attributes.AddRange(attributes);
                SeqEnd = objectCache.GetObject<DwgSeqEnd>(reader, _seqEndHandle.HandleOrOffset);
            }
        }

        internal override void OnBeforeEntityWrite()
        {
            _blockHeaderHandle = BlockHeader.Handle;
            DwgEntityHelpers.PopulateEntityPointers(Attributes, ref _firstAttribHandle, ref _lastAttribHandle, Layer);
            _hasAttributes = Attributes.Count > 0;
            SeqEnd.Layer = Layer;
            _seqEndHandle = SeqEnd.Handle;
        }

        internal override void WritePostData(BitWriter writer)
        {
            writer.Write_H(new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, _blockHeaderHandle.HandleOrOffset));
            if (_hasAttributes)
            {
                writer.Write_H(new DwgHandleReference(DwgHandleReferenceCode.HardPointer, _firstAttribHandle.HandleOrOffset));
                writer.Write_H(new DwgHandleReference(DwgHandleReferenceCode.HardPointer, _lastAttribHandle.HandleOrOffset));
                writer.Write_H(new DwgHandleReference(DwgHandleReferenceCode.SoftPointer, _seqEndHandle.HandleOrOffset));
            }
        }
    }
}
