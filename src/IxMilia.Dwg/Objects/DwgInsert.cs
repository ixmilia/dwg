using System.Collections.Generic;
using System.Linq;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgInsert
    {
        public DwgBlockHeader BlockHeader { get; set; }
        public DwgSeqEnd SeqEnd { get; private set; } = new DwgSeqEnd();
        public List<DwgAttribute> Attributes { get; } = new List<DwgAttribute>();

        internal DwgHandleReference _firstAttribHandleReference;
        internal DwgHandleReference _lastAttribHandleReference;
        internal DwgHandleReference _blockHeaderHandleReference;
        internal DwgHandleReference _seqEndHandleReference;

        internal override IEnumerable<DwgObject> ChildItems
        {
            get
            {
                yield return BlockHeader;
                foreach (var attrib in Attributes)
                {
                    yield return attrib;
                }

                if (Attributes.Any())
                {
                    yield return SeqEnd;
                }
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

        internal override void ReadPostData(BitReader reader, DwgVersionId version)
        {
            _blockHeaderHandleReference = reader.Read_H();
            if (_blockHeaderHandleReference.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect block header handle code.");
            }

            if (_hasAttributes)
            {
                _firstAttribHandleReference = reader.Read_H();
                if (_firstAttribHandleReference.Code != DwgHandleReferenceCode.HardPointer)
                {
                    throw new DwgReadException("Incorrect attribute handle code.");
                }

                _lastAttribHandleReference = reader.Read_H();
                if (_lastAttribHandleReference.Code != DwgHandleReferenceCode.HardPointer)
                {
                    throw new DwgReadException("Incorrect attribute handle code.");
                }

                _seqEndHandleReference = reader.Read_H();
                if (_seqEndHandleReference.Code != DwgHandleReferenceCode.SoftPointer)
                {
                    throw new DwgReadException("Incorrect seqend handle code.");
                }
            }
        }

        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            BlockHeader = objectCache.GetObject<DwgBlockHeader>(reader, ResolveHandleReference(_blockHeaderHandleReference));
            Attributes.Clear();
            if (_hasAttributes)
            {
                var attributes = DwgEntityHelpers.EntitiesFromHandlePointer<DwgAttribute>(objectCache, reader, Handle, _firstAttribHandleReference);
                Attributes.AddRange(attributes);
                SeqEnd = objectCache.GetObject<DwgSeqEnd>(reader, ResolveHandleReference(_seqEndHandleReference));
            }
        }

        internal override void OnBeforeEntityWrite(DwgVersionId version)
        {
            _blockHeaderHandleReference = BlockHeader.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            DwgEntityHelpers.PopulateEntityPointers(Attributes, ref _firstAttribHandleReference, ref _lastAttribHandleReference, Layer);
            _hasAttributes = Attributes.Count > 0;
            SeqEnd.Layer = Layer;
            _seqEndHandleReference = SeqEnd.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
        }

        internal override void WritePostData(BitWriter writer, DwgVersionId version)
        {
            writer.Write_H(_blockHeaderHandleReference);
            if (_hasAttributes)
            {
                writer.Write_H(_firstAttribHandleReference);
                writer.Write_H(_lastAttribHandleReference);
                writer.Write_H(_seqEndHandleReference);
            }
        }
    }
}
