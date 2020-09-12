using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgPolyline3D
    {
        public DwgSeqEnd SeqEnd { get; private set; } = new DwgSeqEnd();
        public List<DwgVertex3D> Vertices { get; } = new List<DwgVertex3D>();

        private DwgHandleReference _firstVertexHandle;
        private DwgHandleReference _lastVertexHandle;
        private DwgHandleReference _seqEndHandle;

        public bool IsClosed
        {
            get => DwgEntityHelpers.HasFlag(_flags2, 0b00000001);
            set => _flags2 = DwgEntityHelpers.WithFlag(value, _flags2, 0b00000001);
        }

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

        public DwgPolyline3D(IEnumerable<DwgVertex3D> vertices)
            : this()
        {
            Vertices.AddRange(vertices);
        }

        public DwgPolyline3D(params DwgVertex3D[] vertices)
            : this((IEnumerable<DwgVertex3D>)vertices)
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
            var vertices = DwgEntityHelpers.EntitiesFromHandlePointer<DwgVertex3D>(objectCache, reader, _firstVertexHandle);
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
