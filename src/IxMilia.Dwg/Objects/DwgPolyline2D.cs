using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgPolyline2D
    {
        public DwgSeqEnd SeqEnd { get; private set; } = new DwgSeqEnd();
        public List<DwgVertex2D> Vertices { get; } = new List<DwgVertex2D>();

        internal DwgHandleReference _firstVertexHandle;
        internal DwgHandleReference _lastVertexHandle;
        internal DwgHandleReference _seqEndHandle;

        internal override IEnumerable<DwgObject> ChildItems
        {
            get
            {
                foreach (var vertex in Vertices)
                {
                    yield return vertex;
                }

                yield return SeqEnd;
            }
        }

        public DwgPolyline2D(IEnumerable<DwgVertex2D> vertices)
            : this()
        {
            Vertices.AddRange(vertices);
        }

        public DwgPolyline2D(params DwgVertex2D[] vertices)
            : this((IEnumerable<DwgVertex2D>)vertices)
        {
        }

        internal override void ReadPostData(BitReader reader)
        {
            _firstVertexHandle = reader.Read_H();
            if (_firstVertexHandle.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect vertex handle code.");
            }

            _lastVertexHandle = reader.Read_H();
            if (_lastVertexHandle.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect vertex handle code.");
            }

            _seqEndHandle = reader.Read_H();
            if (_seqEndHandle.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect seqend handle code.");
            }
        }

        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache)
        {
            Vertices.Clear();
            var vertices = DwgEntityHelpers.EntitiesFromHandlePointer<DwgVertex2D>(objectCache, reader, _firstVertexHandle);
            Vertices.AddRange(vertices);
            SeqEnd = objectCache.GetObject<DwgSeqEnd>(reader, _seqEndHandle.HandleOrOffset);
        }

        internal override void OnBeforeEntityWrite()
        {
            DwgEntityHelpers.PopulateEntityPointers(Vertices, ref _firstVertexHandle, ref _lastVertexHandle, Layer);
            SeqEnd.Layer = Layer;
            _seqEndHandle = SeqEnd.Handle;
        }

        internal override void WritePostData(BitWriter writer, DwgObjectMap objectMap, int pointerOffset)
        {
            writer.Write_H(new DwgHandleReference(DwgHandleReferenceCode.HardPointer, _firstVertexHandle.HandleOrOffset));
            writer.Write_H(new DwgHandleReference(DwgHandleReferenceCode.HardPointer, _lastVertexHandle.HandleOrOffset));
            writer.Write_H(new DwgHandleReference(DwgHandleReferenceCode.SoftPointer, _seqEndHandle.HandleOrOffset));
        }
    }
}
