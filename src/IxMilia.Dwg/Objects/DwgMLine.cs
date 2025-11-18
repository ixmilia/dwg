using System;
using System.Collections.Generic;
using System.Linq;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgMLine
    {
        internal DwgHandleReference _mlineStyleHandleReference;

        internal DwgMLineStyle MLineStyle { get; set; } = new DwgMLineStyle();

        internal override IEnumerable<DwgObject> ChildItems
        {
            get
            {
                yield return MLineStyle;
            }
        }

        internal override void ParseSpecific(BitReader reader, int objectBitOffsetStart, DwgVersionId version)
        {
            Scale = reader.Read_BD();
            Justification = (DwgJustification)reader.Read_EC();
            BasePoint = Converters.TriplePoint(reader.Read_3BD());
            Extrusion = Converters.TripleVector(reader.Read_3BD());
            IsClosed = reader.Read_BS() == 3;
            var lineStyleCount = reader.Read_RC();
            var vertexCount = reader.Read_BS();
            for (int i = 0; i < vertexCount; i++)
            {
                var vertexLocation = Converters.TriplePoint(reader.Read_3BD());
                var vertexDirection = Converters.TripleVector(reader.Read_3BD());
                var miterDirection = Converters.TripleVector(reader.Read_3BD());
                var vertexStyles = new List<DwgMLineVertexStyle>();
                for (int j = 0; j < lineStyleCount; j++)
                {
                    var segmentParameters = new List<double>();
                    var segmentParameterCount = reader.Read_BS();
                    for (int k = 0; k < segmentParameterCount; k++)
                    {
                        var segmentParameter = reader.Read_BD();
                        segmentParameters.Add(segmentParameter);
                    }

                    var areaFillParameters = new List<double>();
                    var areaFillParameterCount = reader.Read_BS();
                    for (int k = 0; k < areaFillParameterCount; k++)
                    {
                        var areaFillParameter = reader.Read_BD();
                        areaFillParameters.Add(areaFillParameter);
                    }

                    var vertexStyle = new DwgMLineVertexStyle(segmentParameters, areaFillParameters);
                    vertexStyles.Add(vertexStyle);
                }

                var vertex = new DwgMLineVertex(vertexLocation, vertexDirection, miterDirection, vertexStyles);
                Vertices.Add(vertex);
            }

            AssertObjectSize(reader, objectBitOffsetStart);
        }

        internal override void ReadPostData(BitReader reader, DwgVersionId version)
        {
            _mlineStyleHandleReference = reader.Read_H();
            if (_mlineStyleHandleReference.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect mline style handle code");
            }
        }

        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            MLineStyle = objectCache.GetObject<DwgMLineStyle>(reader, ResolveHandleReference(_mlineStyleHandleReference));
        }

        internal override void OnBeforeEntityWrite(DwgVersionId version)
        {
            _mlineStyleHandleReference = MLineStyle.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
        }

        internal override void WriteSpecific(BitWriter writer, DwgVersionId version)
        {
            writer.Write_BD(Scale);
            writer.Write_EC((byte)Justification);
            writer.Write_3BD(Converters.TriplePoint(BasePoint));
            writer.Write_3BD(Converters.TripleVector(Extrusion));
            writer.Write_BS((short)(IsClosed ? 3 : 1));

            var lineStyleCounts = Vertices.Select(v => v.Styles.Count).Distinct().ToArray();
            if (lineStyleCounts.Length != 1)
            {
                throw new InvalidOperationException("All mline vertex style counts must be equal");
            }

            var lineStyleCount = lineStyleCounts[0];
            writer.Write_RC((byte)lineStyleCount);
            writer.Write_BS((short)Vertices.Count);
            foreach (var vertex in Vertices)
            {
                writer.Write_3BD(Converters.TriplePoint(vertex.Location));
                writer.Write_3BD(Converters.TripleVector(vertex.VertexDirection));
                writer.Write_3BD(Converters.TripleVector(vertex.MiterDirection));
                foreach (var style in vertex.Styles)
                {
                    writer.Write_BS((short)style.SegmentParameters.Count);
                    foreach (var segmentParameter in style.SegmentParameters)
                    {
                        writer.Write_BD(segmentParameter);
                    }

                    writer.Write_BS((short)style.AreaFillParameters.Count);
                    foreach (var areaFillParameter in style.AreaFillParameters)
                    {
                        writer.Write_BD(areaFillParameter);
                    }
                }
            }

            _objectSize = writer.BitCount;
        }

        internal override void WritePostData(BitWriter writer, DwgVersionId version)
        {
            writer.Write_H(_mlineStyleHandleReference);
        }
    }
}
