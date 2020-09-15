using IxMilia.Dwg.Objects;
using Xunit;

namespace IxMilia.Dwg.Test
{
    /// <summary>
    /// All tests in this class parse the raw bits (e.g., no pointers are bound) as given in the example PDF spec.
    /// </summary>
    public class RawObjectTests : AbstractReaderTests
    {
        public static DwgObject ParseRaw(params int[] data)
        {
            var reader = Bits(data);
            var obj = DwgObject.ParseRaw(reader, DwgVersionId.R14);
            return obj;
        }

        [Fact]
        public void ReadRawText()
        {
            var text = (DwgText)ParseRaw(
                0x49, 0x00,                                     // length
                0x40, 0x40, 0x53, 0x20, 0x58, 0x10, 0x00, 0x05, // data
                0x5B, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
                0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02,
                0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x14, 0xD4, 0x4D, 0x4C, 0xCC, 0xCC, 0xCC,
                0xCC, 0xE4, 0x9F, 0xA8, 0x63, 0xA3, 0x43, 0x4B,
                0x99, 0x03, 0x4B, 0x99, 0x03, 0xA3, 0x2B, 0xC3,
                0xA5, 0x46, 0x0A, 0x21, 0xE8, 0x08, 0x0A, 0x22,
                0x00,
                0xC9, 0x72                                      // crc
            );
            Assert.Equal(0x4C, text.Handle.HandleOrOffset);
            Assert.Equal(0.0, text.Elevation);
            Assert.Equal(new DwgPoint(3.0, 4.0, 0.0), text.InsertionPoint);
            Assert.Equal(DwgPoint.Origin, text.AlignmentPoint);
            Assert.Equal(DwgVector.ZAxis, text.Extrusion);
            Assert.Equal(0.0, text.Thickness);
            Assert.Equal(0.0, text.ObliqueAngle);
            Assert.Equal(0.0, text.RotationAngle);
            Assert.Equal(0.2, text.Height);
            Assert.Equal(1.0, text.WidthFactor);
            Assert.Equal("this is text", text.Value);
            Assert.False(text.IsTextBackward);
            Assert.False(text.IsTextUpsideDown);
            Assert.Equal(DwgHorizontalTextJustification.Left, text.HorizontalAlignment);
            Assert.Equal(DwgVerticalTextJustification.Baseline, text.VerticalAlignment);
        }

        [Fact]
        public void ReadRawAttribute()
        {
            var attr = (DwgAttribute)ParseRaw(
                0x58, 0x00,                                     // length
                0x40, 0x80, 0x54, 0xA3, 0xF8, 0x10, 0x00, 0x01, // data
                0x5B, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02,
                0x88, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03,
                0x88, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x14, 0xD4, 0x4D, 0xCC, 0xCC, 0xCC, 0xCC,
                0xCC, 0xE4, 0x9F, 0x9F, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFD, 0xE7, 0xE8, 0x5B, 0x6B, 0xCB, 0x0B,
                0xA3, 0xA1, 0x03, 0xB3, 0x0B, 0x63, 0xAB, 0x2D,
                0x48, 0x2A, 0x6A, 0xCA, 0x0A, 0xA2, 0xA4, 0x01,
                0x00, 0x60, 0xA2, 0x1E, 0x80, 0x80, 0xA2, 0x21,
                0x6F, 0xA6                                      // crc
            );
            Assert.Equal(0x52, attr.Handle.HandleOrOffset);
            Assert.Equal(0.0, attr.Elevation);
            Assert.Equal(new DwgPoint(5.0, 7.0, 0.0), attr.InsertionPoint);
            Assert.Equal(DwgPoint.Origin, attr.AlignmentPoint);
            Assert.Equal(DwgVector.ZAxis, attr.Extrusion);
            Assert.Equal(0.0, attr.Thickness);
            Assert.Equal(0.0, attr.ObliqueAngle);
            Assert.Equal(0.0, attr.RotationAngle);
            Assert.Equal(0.20000000000000004, attr.Height);
            Assert.Equal(0.9999999999999999, attr.WidthFactor);
            Assert.Equal("myatt value", attr.Value);
            Assert.Equal(0, attr.Generation);
            Assert.Equal(DwgHorizontalTextJustification.Left, attr.HorizontalAlignment);
            Assert.Equal(DwgVerticalTextJustification.Baseline, attr.VerticalAlignment);
            Assert.Equal("MYATT", attr.Tag);
            Assert.Equal(0, attr.FieldLength);
            Assert.Equal(0, attr.Flags);
        }

        [Fact]
        public void ReadRawAttributeDefinition()
        {
            var attdef = (DwgAttributeDefinition)ParseRaw(
                0x50, 0x00,                                     // length
                0x40, 0xC0, 0x53, 0x22, 0x08, 0x10, 0x00, 0x05, // data
                0x5B, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
                0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02,
                0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x14, 0xD4, 0x4D, 0x4C, 0xCC, 0xCC, 0xCC,
                0xCC, 0xE4, 0x9F, 0xB5, 0x48, 0x2A, 0x6A, 0xCA,
                0x0A, 0xA2, 0xA4, 0x00, 0x85, 0xA2, 0xB7, 0x3A,
                0x32, 0xB9, 0x10, 0x36, 0xBC, 0xB0, 0xBA, 0x3A,
                0x18, 0x28, 0x87, 0xA0, 0x20, 0x28, 0x88, 0x00,
                0x78, 0x53                                      // crc
            );
            Assert.Equal(0x4C, attdef.Handle.HandleOrOffset);
            Assert.Equal(0.0, attdef.Elevation);
            Assert.Equal(new DwgPoint(3.0, 4.0, 0.0), attdef.InsertionPoint);
            Assert.Equal(DwgPoint.Origin, attdef.AlignmentPoint);
            Assert.Equal(DwgVector.ZAxis, attdef.Extrusion);
            Assert.Equal(0.0, attdef.Thickness);
            Assert.Equal(0.0, attdef.ObliqueAngle);
            Assert.Equal(0.0, attdef.RotationAngle);
            Assert.Equal(0.2, attdef.Height);
            Assert.Equal(1.0, attdef.WidthFactor);
            Assert.Equal("", attdef.DefaultValue);
            Assert.Equal(0, attdef.Generation);
            Assert.Equal(DwgHorizontalTextJustification.Left, attdef.HorizontalAlignment);
            Assert.Equal(DwgVerticalTextJustification.Baseline, attdef.VerticalAlignment);
            Assert.Equal("MYATT", attdef.Tag);
            Assert.Equal(0, attdef.FieldLength);
            Assert.Equal(0, attdef.Flags);
            Assert.Equal("Enter myatt", attdef.Prompt);
        }

        [Fact]
        public void ReadRawBlock()
        {
            var block = (DwgBlock)ParseRaw(
                0x16, 0x00,                                     // length
                0x41, 0x00, 0x53, 0xA3, 0xD8, 0x00, 0x00, 0x01, // data
                0x5B, 0x20, 0xA9, 0xAB, 0x28, 0x49, 0x89, 0x70,
                0x06, 0x0A, 0x21, 0xE8, 0x08, 0x00,
                0x39, 0xF3                                      // crc
            );
            Assert.Equal(0x4E, block.Handle.HandleOrOffset);
            Assert.Equal("MYBLK", block.Name);
        }

        [Fact]
        public void ReadRawEndBlock()
        {
            var endblock = (DwgEndBlock)ParseRaw(
                0x0F, 0x00,                                     // length
                0x41, 0x40, 0x46, 0xE2, 0x48, 0x00, 0x00, 0x05, // data
                0x5B, 0x18, 0x28, 0x87, 0xA0, 0x20, 0x20,
                0x2E, 0x8B                                      // crc
            );
            Assert.Equal(0x1B, endblock.Handle.HandleOrOffset);
        }

        [Fact]
        public void ReadRawSeqend()
        {
            var seqend = (DwgSeqEnd)ParseRaw(
                0x11, 0x00,                                     // length
                0x41, 0x80, 0x54, 0xE2, 0x48, 0x00, 0x00, 0x01, // data
                0x5B, 0x60, 0x81, 0x18, 0x28, 0x87, 0xA0, 0x20,
                0x08,
                0x88, 0xC7                                      // crc
            );
            Assert.Equal(0x53, seqend.Handle.HandleOrOffset);
        }

        [Fact]
        public void ReadRawInsert()
        {
            var insert = (DwgInsert)ParseRaw(
                0x29, 0x00,                                     // length
                0x41, 0xC0, 0x54, 0x66, 0xF0, 0x00, 0x00, 0x05, // data
                0x5B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
                0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x82, 0x04, 0xAD, 0x4C, 0xC1, 0x44, 0x3D, 0x01,
                0x01, 0x45, 0x35, 0x05, 0x49, 0x05, 0x48, 0xC5,
                0x4C,
                0xCB, 0x54                                      // crc
            );
            Assert.Equal(0x51, insert.Handle.HandleOrOffset);
            Assert.Equal(new DwgPoint(3.0, 4.0, 0.0), insert.Location);
            Assert.Equal(1.0, insert.XScale);
            Assert.Equal(1.0, insert.YScale);
            Assert.Equal(1.0, insert.ZScale);
            Assert.Equal(0.0, insert.Rotation);
            Assert.Equal(DwgVector.ZAxis, insert.Extrusion);
            Assert.True(insert._hasAttributes);
            Assert.Equal(0x4D, insert._blockHeaderHandle.HandleOrOffset);
            Assert.Equal(0x52, insert._firstAttribHandle.HandleOrOffset);
            Assert.Equal(0x52, insert._lastAttribHandle.HandleOrOffset);
            Assert.Equal(0x53, insert._seqEndHandle.HandleOrOffset);
        }

        [Fact]
        public void ReadRawMInsert()
        {
            var insert = (DwgMInsert)ParseRaw(
                0x36, 0x00,                                     // length
                0x42, 0x00, 0x56, 0x63, 0xB0, 0x08, 0x00, 0x05, // data
                0x5B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x42, 0x04, 0xAD, 0x49, 0x04, 0x40, 0xC0, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x84, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0xC1,
                0x44, 0x3D, 0x01, 0x01, 0x45, 0x44,
                0x84, 0x2E                                      // crc
            );
            Assert.Equal(0x59, insert.Handle.HandleOrOffset);
            Assert.Equal(new DwgPoint(2.0, 3.0, 0.0), insert.Location);
            Assert.Equal(1.0, insert.XScale);
            Assert.Equal(1.0, insert.YScale);
            Assert.Equal(1.0, insert.ZScale);
            Assert.Equal(0.0, insert.Rotation);
            Assert.Equal(DwgVector.ZAxis, insert.Extrusion);
            Assert.False(insert._hasAttributes);
            Assert.Equal(4, insert.ColumnCount);
            Assert.Equal(3, insert.RowCount);
            Assert.Equal(3.0, insert.ColumnSpacing);
            Assert.Equal(2.0, insert.RowSpacing);
            Assert.Equal(0x51, insert._blockHeaderHandle.HandleOrOffset);
            Assert.Equal(0x00, insert._firstAttribHandle.HandleOrOffset);
            Assert.Equal(0x00, insert._lastAttribHandle.HandleOrOffset);
            Assert.Equal(0x00, insert._seqEndHandle.HandleOrOffset);
        }

        [Fact]
        public void ReadRawVertex2D()
        {
            var vert = (DwgVertex2D)ParseRaw(
                0x22, 0x00,                                     // length
                0x42, 0x80, 0x53, 0x66, 0xF8, 0x00, 0x00, 0x01, // data
                0x5B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x02, 0x05, 0x55, 0x00, 0x60, 0xA2, 0x1E,
                0x80, 0xC1,
                0xB2, 0xFC                                      // crc
            );
            Assert.Equal(0x4D, vert.Handle.HandleOrOffset);
            Assert.Equal(0, vert.Flags);
            Assert.Equal(new DwgPoint(2.0, 2.0, 0.0), vert.Point);
            Assert.Equal(0.0, vert.StartWidth);
            Assert.Equal(0.0, vert.EndWidth);
            Assert.Equal(0.0, vert.Bulge);
            Assert.Equal(0.0, vert.TangentDirection);
        }

        [Fact]
        public void ReadRawVertex3D()
        {
            var vert = (DwgVertex3D)ParseRaw(
                0x1A, 0x00,                                     // length
                0x42, 0xC0, 0x58, 0xA4, 0xB8, 0x00, 0x00, 0x01, // data
                0x5B, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x08, 0x0D, 0x82, 0x08, 0x60, 0xA2, 0x1F,
                0x00, 0x80,
                0x8C, 0x03                                      // crc
            );
            Assert.Equal(0x62, vert.Handle.HandleOrOffset);
            Assert.Equal(0x20, vert.Flags);
            Assert.Equal(new DwgPoint(2.0, 1.0, 0.0), vert.Point);
        }

        [Fact]
        public void ReadRawVertexMesh()
        {
            var vert = (DwgVertexMesh)ParseRaw(
                0x21, 0x00,                                     // length
                0x43, 0x00, 0x59, 0xE6, 0xB8, 0x00, 0x00, 0x01, // data
                0x5B, 0x20, 0x19, 0x1D, 0x70, 0xD1, 0x7F, 0xE3,
                0xFF, 0x47, 0xE1, 0x72, 0xDB, 0x05, 0xA8, 0xC4,
                0x58, 0xCA, 0x05, 0x00, 0x60, 0xA2, 0x1E, 0x80,
                0xC0,
                0xB3, 0x50                                      // crc
            );
            Assert.Equal(0x67, vert.Handle.HandleOrOffset);
            Assert.Equal(0x40, vert.Flags);
            Assert.Equal(new DwgPoint(1.6328120661043943, 6.385836442953218, 0.0), vert.Point);
        }

        [Fact]
        public void ReadRawVertexPFace()
        {
            var vert = (DwgVertexPFace)ParseRaw(
                0x21, 0x00,                                     // length
                0x43, 0x40, 0x55, 0xA6, 0xB8, 0x00, 0x00, 0x01, // data
                0x5B, 0x60, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x10, 0x81, 0x00, 0x60, 0xA2, 0x1E, 0x80,
                0xC1,
                0x3D, 0x1E                                      // crc
            );
            Assert.Equal(0x56, vert.Handle.HandleOrOffset);
            Assert.Equal(0xC0, vert.Flags);
            Assert.Equal(new DwgPoint(1.0, 2.0, 3.0), vert.Point);
        }

        [Fact]
        public void ReadRawVertexPFaceFace()
        {
            var vert = (DwgVertexPFaceFace)ParseRaw(
                0x13, 0x00,                                     // length
                0x43, 0x80, 0x56, 0xA3, 0x48, 0x00, 0x00, 0x01, // data
                0x7B, 0x20, 0x28, 0x1A, 0x05, 0x60, 0x82, 0x98,
                0x28, 0x87, 0x80,
                0xC3, 0xBA                                      // crc
            );
            Assert.Equal(0x5A, vert.Handle.HandleOrOffset);
            Assert.Equal(1, vert.Index1);
            Assert.Equal(3, vert.Index2);
            Assert.Equal(2, vert.Index3);
            Assert.Equal(0, vert.Index4);
        }

        [Fact]
        public void ReadRawPolyline2D()
        {
            var poly = (DwgPolyline2D)ParseRaw(
                0x18, 0x00,                                     // length
                0x43, 0xC0, 0x53, 0x22, 0xD8, 0x00, 0x00, 0x05,
                0x5B, 0x55, 0x55, 0x26, 0x0A, 0x21, 0xE8, 0x14,
                0x21, 0x28, 0x29, 0xA8, 0x29, 0xE6, 0x2A, 0x01,
                0x13, 0xEA                                      // crc
            );
            Assert.Equal(0x4C, poly.Handle.HandleOrOffset);
            Assert.Equal(0, poly.Flags);
            Assert.Equal(DwgCurveType.None, poly.CurveType);
            Assert.Equal(0.0, poly.StartWidth);
            Assert.Equal(0.0, poly.EndWidth);
            Assert.Equal(0.0, poly.Thickness);
            Assert.Equal(0.0, poly.Elevation);
            Assert.Equal(DwgVector.ZAxis, poly.Extrusion);
            Assert.Equal(0x4D, poly._firstVertexHandle.HandleOrOffset);
            Assert.Equal(0x4F, poly._lastVertexHandle.HandleOrOffset);
            Assert.Equal(0x50, poly._seqEndHandle.HandleOrOffset);
        }

        [Fact]
        public void ReadRawPolyline3D()
        {
            var poly = (DwgPolyline3D)ParseRaw(
                0x19, 0x00,                                     // length
                0x44, 0x00, 0x57, 0xA2, 0xC8, 0x00, 0x00, 0x05, // data
                0x5B, 0x00, 0x00, 0x18, 0x28, 0x87, 0xE0, 0x84,
                0xD0, 0x83, 0x20, 0xAF, 0xA0, 0xB1, 0x18, 0xB1,
                0x80,
                0x4A, 0xA6                                      // crc
            );
            Assert.Equal(0x5E, poly.Handle.HandleOrOffset);
            Assert.Equal(0, poly._flags1);
            Assert.Equal(0, poly._flags2);
            Assert.Equal(0x5F, poly._firstVertexHandle.HandleOrOffset);
            Assert.Equal(0x62, poly._lastVertexHandle.HandleOrOffset);
            Assert.Equal(0x63, poly._seqEndHandle.HandleOrOffset);
        }

        [Fact]
        public void ReadRawArc()
        {
            var arc = (DwgArc)ParseRaw(
                0x3A, 0x00,                                     // length
                0x44, 0x40, 0x59, 0x24, 0xE8, 0x08, 0x00, 0x05, // data
                0x5B, 0x0F, 0x61, 0xAA, 0x41, 0xEB, 0xF9, 0xA0,
                0x88, 0x05, 0xDD, 0x50, 0x53, 0x3A, 0x0A, 0x70,
                0xEA, 0x04, 0x13, 0xB4, 0xFD, 0xAC, 0x6D, 0xCB,
                0x7A, 0x9F, 0xD4, 0x88, 0x6D, 0xE1, 0xF9, 0xBC,
                0xBC, 0x60, 0x08, 0x00, 0x27, 0x5B, 0x70, 0xE5,
                0x02, 0x68, 0x7A, 0x01, 0x82, 0x88, 0x7E, 0x08,
                0x33, 0x05,
                0x91, 0x5F                                      // crc
            );
            Assert.Equal(0x64, arc.Handle.HandleOrOffset);
            Assert.Equal(new DwgPoint(2.600278968516873, 7.326421365762765, 0.0), arc.Center);
            Assert.Equal(1.3493302799093472, arc.Radius);
            Assert.Equal(0.0, arc.Thickness);
            Assert.Equal(DwgVector.ZAxis, arc.Extrusion);
            Assert.Equal(2.111278154405143, arc.StartAngle);
            Assert.Equal(3.9129030438756462, arc.EndAngle);
        }

        [Fact]
        public void ReadRawCircle()
        {
            var circle = (DwgCircle)ParseRaw(
                0x2B, 0x00,                                     // length
                0x44, 0x80, 0x64, 0xA0, 0xC8, 0x08, 0x00, 0x05, // data
                0x5B, 0x0A, 0x88, 0xA1, 0xBF, 0x90, 0x3F, 0xC3,
                0x48, 0x00, 0x45, 0x2D, 0xC2, 0xC7, 0x6F, 0x28,
                0xFA, 0x04, 0x6A, 0x9D, 0xCD, 0x75, 0xA2, 0x1A,
                0x72, 0x9F, 0xD4, 0x98, 0x28, 0x87, 0xE0, 0x96,
                0x50, 0x86, 0x6D,
                0x36, 0x1C                                      // crc
            );
            Assert.Equal(0x92, circle.Handle.HandleOrOffset);
            Assert.Equal(new DwgPoint(6.7485427268123495, 7.9745382178018716, 0.0), circle.Center);
            Assert.Equal(0.6626305200205659, circle.Radius);
            Assert.Equal(0.0, circle.Thickness);
            Assert.Equal(DwgVector.ZAxis, circle.Extrusion);
        }

        [Fact]
        public void ReadRawLine()
        {
            var line = (DwgLine)ParseRaw(
                0x35, 0x00,                                     // length
                0x44, 0xC0, 0x73, 0x22, 0xE8, 0x08, 0x00, 0x01, // data
                0x13, 0x00, 0x6B, 0xB5, 0x95, 0xB2, 0xD9, 0x24,
                0x08, 0x04, 0x88, 0x93, 0xFD, 0xFD, 0x9A, 0x00,
                0xFA, 0x04, 0x53, 0xE6, 0xF4, 0xDB, 0xB6, 0xB6,
                0x90, 0x20, 0x12, 0x02, 0x4F, 0xF7, 0xF6, 0x68,
                0x03, 0xE8, 0x15, 0x4E, 0x08, 0x11, 0x82, 0x88,
                0x7A, 0x88, 0x9A, 0x03, 0x06,
                0xFA, 0xFE                                      // crc
            );
            Assert.Equal(0xCC, line.Handle.HandleOrOffset);
            Assert.Equal(new DwgPoint(8.393727710047193, 7.813185684328929, 0.0), line.P1);
            Assert.Equal(new DwgPoint(8.213727710047193, 7.8131856843289285, 0.0), line.P2);
            Assert.Equal(0.0, line.Thickness);
            Assert.Equal(DwgVector.ZAxis, line.Extrusion);
        }
    }
}
