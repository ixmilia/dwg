#nullable enable

using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgPolyline2D
    {
        public DwgSeqEnd SeqEnd { get; private set; } = new DwgSeqEnd();
        public List<DwgVertex2D> Vertices { get; } = new List<DwgVertex2D>();

        internal DwgHandleReference _firstVertexHandleReference;
        internal DwgHandleReference _lastVertexHandleReference;
        internal DwgHandleReference _seqEndHandleReference;

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

        internal override void ReadPostData(BitReader reader, DwgVersionId version)
        {
            _firstVertexHandleReference = reader.Read_H();
            if (_firstVertexHandleReference.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect vertex handle code.");
            }

            _lastVertexHandleReference = reader.Read_H();
            if (_lastVertexHandleReference.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect vertex handle code.");
            }

            _seqEndHandleReference = reader.Read_H();
            if (_seqEndHandleReference.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect seqend handle code.");
            }
        }

        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            Vertices.Clear();
            var vertices = DwgEntityHelpers.EntitiesFromHandlePointer<DwgVertex2D>(objectCache, reader, Handle, _firstVertexHandleReference);
            Vertices.AddRange(vertices);
            SeqEnd = objectCache.GetObject<DwgSeqEnd>(reader, ResolveHandleReference(_seqEndHandleReference));
        }

        internal override void OnBeforeEntityWrite(DwgVersionId version)
        {
            DwgEntityHelpers.PopulateEntityPointers(Vertices, ref _firstVertexHandleReference, ref _lastVertexHandleReference, Layer);
            SeqEnd.Layer = Layer;
            _seqEndHandleReference = SeqEnd.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
        }

        internal override void WritePostData(BitWriter writer, DwgVersionId version)
        {
            writer.Write_H(_firstVertexHandleReference);
            writer.Write_H(_lastVertexHandleReference);
            writer.Write_H(_seqEndHandleReference);
        }
    }
}
