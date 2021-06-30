using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgPolylineMesh
    {
        public DwgSeqEnd SeqEnd { get; private set; } = new DwgSeqEnd();
        public List<DwgVertexMesh> Vertices { get; } = new List<DwgVertexMesh>();

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

        public DwgPolylineMesh(IEnumerable<DwgVertexMesh> vertices)
            : this()
        {
            Vertices.AddRange(vertices);
        }

        public DwgPolylineMesh(params DwgVertexMesh[] vertices)
            : this((IEnumerable<DwgVertexMesh>)vertices)
        {
        }

        internal override void ReadPostData(BitReader reader)
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

        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache)
        {
            Vertices.Clear();
            var vertices = DwgEntityHelpers.EntitiesFromHandlePointer<DwgVertexMesh>(objectCache, reader, Handle, _firstVertexHandleReference);
            Vertices.AddRange(vertices);
            SeqEnd = objectCache.GetObject<DwgSeqEnd>(reader, ResolveHandleReference(_seqEndHandleReference));
        }

        internal override void OnBeforeEntityWrite()
        {
            DwgEntityHelpers.PopulateEntityPointers(Vertices, ref _firstVertexHandleReference, ref _lastVertexHandleReference, Layer);
            SeqEnd.Layer = Layer;
            _seqEndHandleReference = SeqEnd.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
        }

        internal override void WritePostData(BitWriter writer)
        {
            writer.Write_H(_firstVertexHandleReference);
            writer.Write_H(_lastVertexHandleReference);
            writer.Write_H(_seqEndHandleReference);
        }
    }
}
