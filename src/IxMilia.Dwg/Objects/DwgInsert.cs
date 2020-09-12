using System.Collections.Generic;
using System.Linq;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgInsert
    {
        public DwgBlockHeader BlockHeader { get; set; }
        public DwgSeqEnd SeqEnd { get; private set; } = new DwgSeqEnd();
        public List<DwgAttribute> Attributes { get; } = new List<DwgAttribute>();

        private DwgHandleReference _firstAttribHandle;
        private DwgHandleReference _lastAttribHandle;
        private DwgHandleReference _blockHeaderHandle;
        private DwgHandleReference _seqEndHandle;

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
                var currentAttribHandle = _firstAttribHandle;
                while (!currentAttribHandle.PointsToNull)
                {
                    var attrib = objectCache.GetObject<DwgAttribute>(reader, currentAttribHandle.HandleOrOffset);
                    Attributes.Add(attrib);
                    currentAttribHandle = attrib.Handle.GetNextHandle(attrib.NextEntityHandle);
                }

                SeqEnd = objectCache.GetObject<DwgSeqEnd>(reader, _seqEndHandle.HandleOrOffset);
            }
        }

        internal override void OnBeforeEntityWrite()
        {
            _blockHeaderHandle = BlockHeader.Handle;

            for (int i = 0; i < Attributes.Count; i++)
            {
                var currentAttribute = Attributes[i];
                currentAttribute.Layer = Layer;
                var previousAttribute = i == 0
                    ? null
                    : Attributes[i - 1];
                var nextAttribute = i == Attributes.Count - 1
                    ? null
                    : Attributes[i + 1];
                currentAttribute.PreviousEntityHandle = currentAttribute.GetHandleToObject(previousAttribute, DwgHandleReferenceCode.HardPointer);
                currentAttribute.NextEntityHandle = currentAttribute.GetHandleToObject(nextAttribute, DwgHandleReferenceCode.HardPointer);
            }

            if (Attributes.Count == 0)
            {
                _hasAttributes = false;
                _firstAttribHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, 0);
                _lastAttribHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, 0);
            }
            else
            {
                _hasAttributes = true;
                _firstAttribHandle = Attributes.First().Handle;
                _lastAttribHandle = Attributes.Last().Handle;
            }

            SeqEnd.Layer = Layer;
            _seqEndHandle = SeqEnd.Handle;
        }

        internal override void WritePostData(BitWriter writer, DwgObjectMap objectMap, int pointerOffset)
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
