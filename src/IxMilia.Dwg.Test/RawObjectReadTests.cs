﻿using System;
using System.Collections.Generic;
using System.Linq;
using IxMilia.Dwg.Objects;
using Xunit;

namespace IxMilia.Dwg.Test
{
    /// <summary>
    /// All tests in this class parse the raw bits (e.g., no pointers are bound) as given in the example PDF spec.
    /// </summary>
    public class RawObjectReadTests : AbstractReaderTests
    {
        private static DwgClassDefinition[] CommonClasses()
        {
            return new DwgClassDefinition[]
            {
                null,
                new DwgClassDefinition(0, 0, "", "", "RASTERVARIABLES", false, false),
                null,
                new DwgClassDefinition(0, 0, "", "", "IMAGEDEF", false, false),
                new DwgClassDefinition(0, 0, "", "", "IMAGEDEFREACTOR", false, false),
                new DwgClassDefinition(0, 0, "", "", "IMAGE", false, false),
                new DwgClassDefinition(0, 0, "", "", "IDBUFFER", false, false),
                new DwgClassDefinition(0, 0, "", "", "LWPLINE", false, false),
                null,
                new DwgClassDefinition(0, 0, "", "", "SPATIAL_FILTER", false, false),
                null,
                new DwgClassDefinition(0, 0, "", "", "LAYER_INDEX", false, false)
            };
        }

        public static DwgObject ParseRaw(DwgClassDefinition[] classes, params int[] data)
        {
            var reader = Bits(data);
            var obj = DwgObject.ParseRaw(reader, DwgVersionId.R14, classes);
            return obj;
        }

        public static DwgObject ParseRaw(params int[] data)
        {
            return ParseRaw(CommonClasses(), data);
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
            Assert.Equal(new DwgHandle(0x4C), text.Handle);
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
            Assert.Equal(new DwgHandle(0x52), attr.Handle);
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
            Assert.Equal(new DwgHandle(0x4C), attdef.Handle);
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
            Assert.Equal(new DwgHandle(0x4E), block.Handle);
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
            Assert.Equal(new DwgHandle(0x1B), endblock.Handle);
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
            Assert.Equal(new DwgHandle(0x53), seqend.Handle);
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
            Assert.Equal(new DwgHandle(0x51), insert.Handle);
            Assert.Equal(new DwgPoint(3.0, 4.0, 0.0), insert.Location);
            Assert.Equal(1.0, insert.XScale);
            Assert.Equal(1.0, insert.YScale);
            Assert.Equal(1.0, insert.ZScale);
            Assert.Equal(0.0, insert.Rotation);
            Assert.Equal(DwgVector.ZAxis, insert.Extrusion);
            Assert.True(insert._hasAttributes);
            Assert.Equal(new DwgHandle(0x4D), insert.ResolveHandleReference(insert._blockHeaderHandleReference));
            Assert.Equal(new DwgHandle(0x52), insert.ResolveHandleReference(insert._firstAttribHandleReference));
            Assert.Equal(new DwgHandle(0x52), insert.ResolveHandleReference(insert._lastAttribHandleReference));
            Assert.Equal(new DwgHandle(0x53), insert.ResolveHandleReference(insert._seqEndHandleReference));
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
            Assert.Equal(new DwgHandle(0x59), insert.Handle);
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
            Assert.Equal(new DwgHandle(0x51), insert.ResolveHandleReference(insert._blockHeaderHandleReference));
            Assert.Equal(new DwgHandle(0x00), insert.ResolveHandleReference(insert._firstAttribHandleReference));
            Assert.Equal(new DwgHandle(0x00), insert.ResolveHandleReference(insert._lastAttribHandleReference));
            Assert.Equal(new DwgHandle(0x00), insert.ResolveHandleReference(insert._seqEndHandleReference));
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
            Assert.Equal(new DwgHandle(0x4D), vert.Handle);
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
            Assert.Equal(new DwgHandle(0x62), vert.Handle);
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
            Assert.Equal(new DwgHandle(0x67), vert.Handle);
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
            Assert.Equal(new DwgHandle(0x56), vert.Handle);
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
            Assert.Equal(new DwgHandle(0x5A), vert.Handle);
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
                0x43, 0xC0, 0x53, 0x22, 0xD8, 0x00, 0x00, 0x05, // data
                0x5B, 0x55, 0x55, 0x26, 0x0A, 0x21, 0xE8, 0x14,
                0x21, 0x28, 0x29, 0xA8, 0x29, 0xE6, 0x2A, 0x01,
                0x13, 0xEA                                      // crc
            );
            Assert.Equal(new DwgHandle(0x4C), poly.Handle);
            Assert.Equal(0, poly.Flags);
            Assert.Equal(DwgCurveType.None, poly.CurveType);
            Assert.Equal(0.0, poly.StartWidth);
            Assert.Equal(0.0, poly.EndWidth);
            Assert.Equal(0.0, poly.Thickness);
            Assert.Equal(0.0, poly.Elevation);
            Assert.Equal(DwgVector.ZAxis, poly.Extrusion);
            Assert.Equal(new DwgHandle(0x4D), poly.ResolveHandleReference(poly._firstVertexHandleReference));
            Assert.Equal(new DwgHandle(0x4F), poly.ResolveHandleReference(poly._lastVertexHandleReference));
            Assert.Equal(new DwgHandle(0x50), poly.ResolveHandleReference(poly._seqEndHandleReference));
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
            Assert.Equal(new DwgHandle(0x5E), poly.Handle);
            Assert.Equal(0, poly._flags1);
            Assert.Equal(0, poly._flags2);
            Assert.Equal(new DwgHandle(0x5F), poly.ResolveHandleReference(poly._firstVertexHandleReference));
            Assert.Equal(new DwgHandle(0x62), poly.ResolveHandleReference(poly._lastVertexHandleReference));
            Assert.Equal(new DwgHandle(0x63), poly.ResolveHandleReference(poly._seqEndHandleReference));
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
            Assert.Equal(new DwgHandle(0x64), arc.Handle);
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
            Assert.Equal(new DwgHandle(0x92), circle.Handle);
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
            Assert.Equal(new DwgHandle(0xCC), line.Handle);
            Assert.Equal(new DwgPoint(8.393727710047193, 7.813185684328929, 0.0), line.P1);
            Assert.Equal(new DwgPoint(8.213727710047193, 7.8131856843289285, 0.0), line.P2);
            Assert.Equal(0.0, line.Thickness);
            Assert.Equal(DwgVector.ZAxis, line.Extrusion);
        }

        [Fact]
        public void ReadRawDimensionOrdinate()
        {
            var dim = (DwgDimensionOrdinate)ParseRaw(
                0x5C, 0x00,                                     // length
                0x45, 0x00, 0x67, 0xA4, 0x08, 0x10, 0x00, 0x05, // data
                0x5B, 0x52, 0x6B, 0x24, 0xC2, 0x1F, 0xB9, 0x8C,
                0x32, 0x80, 0x21, 0x6E, 0x4C, 0x98, 0xC7, 0x73,
                0xF0, 0x7F, 0x05, 0xD4, 0xAC, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x01, 0x50, 0xD8, 0x84,
                0x7F, 0x51, 0xB7, 0x94, 0x26, 0x80, 0x2C, 0x78,
                0x71, 0x23, 0xC3, 0x5B, 0x81, 0x20, 0x40, 0x61,
                0xB6, 0x92, 0x67, 0x34, 0xE8, 0xBA, 0x00, 0x21,
                0x6E, 0x4C, 0x98, 0xC7, 0x73, 0xF0, 0x7F, 0x00,
                0x18, 0x28, 0x87, 0xE0, 0x86, 0x50, 0x87, 0x28,
                0x8E, 0xA8, 0xC9, 0x80,
                0x8E, 0x48                                      // crc
            );
            Assert.Equal(new DwgHandle(0x9E), dim.Handle);
            Assert.Equal(DwgVector.ZAxis, dim.Extrusion);
            Assert.Equal(new DwgPoint(6.444198837595242, 1.5452607130677656, 0.0), dim.TextMidpoint);
            Assert.Equal(0.0, dim.Elevation);
            Assert.Equal(0x0B, dim._flags1);
            Assert.Equal("", dim.Text);
            Assert.Equal(0.0, dim.TextRotation);
            Assert.Equal(0.0, dim.HorizontalDirection);
            Assert.Equal(DwgVector.One, dim.InsertionScale);
            Assert.Equal(0.0, dim.InsertionRotation);
            Assert.Equal(DwgPoint.Origin, dim._unknown);
            Assert.Equal(DwgPoint.Origin, dim.FirstDefinitionPoint);
            Assert.Equal(new DwgPoint(4.948103550778438, 2.339611589062809, 0.0), dim.SecondDefinitionPoint);
            Assert.Equal(new DwgPoint(5.904198837595242, 1.5452607130677656, 0.0), dim.ThirdDefinitionPoint);
            Assert.Equal(0x00, dim._flags2);
            Assert.Equal(new DwgHandle(0x1D), dim.ResolveHandleReference(dim._dimStyleHandleReference));
            Assert.Equal(new DwgHandle(0x93), dim.ResolveHandleReference(dim._anonymousBlockHandleReference));
        }

        [Fact]
        public void ReadRawDimensionLinear()
        {
            var dim = (DwgDimensionLinear)ParseRaw(
                0x6B, 0x00,                                     // length
                0x45, 0x40, 0x6B, 0x27, 0xE8, 0x10, 0x00, 0x05, // data
                0x5B, 0x52, 0xA8, 0x5F, 0xBD, 0x44, 0x3D, 0x70,
                0x3C, 0x80, 0x80, 0x18, 0x62, 0xE8, 0x57, 0x62,
                0x24, 0x81, 0x05, 0xD4, 0xAC, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x72, 0x6E, 0x2A,
                0x01, 0xC0, 0xD2, 0x8D, 0x20, 0x09, 0x11, 0xEC,
                0x04, 0xB1, 0x82, 0x01, 0x48, 0x11, 0xC5, 0x80,
                0x66, 0x42, 0xBC, 0xCA, 0x42, 0x80, 0x5C, 0x7C,
                0xB9, 0x38, 0x1C, 0xBB, 0x05, 0x20, 0x47, 0x16,
                0x01, 0x99, 0x0A, 0xF3, 0x29, 0x0A, 0x00, 0x80,
                0x18, 0x62, 0xE8, 0x57, 0x62, 0x24, 0x81, 0x51,
                0x82, 0x88, 0x7E, 0x08, 0x75, 0x08, 0x72, 0x88,
                0xEA, 0x8C, 0xFB,
                0x48, 0xDA                                      // crc
            );
            Assert.Equal(new DwgHandle(0xAC), dim.Handle);
            Assert.Equal(DwgVector.ZAxis, dim.Extrusion);
            Assert.Equal(new DwgPoint(7.679804367838944, 4.673017325872081, 0.0), dim.TextMidpoint);
            Assert.Equal(0.0, dim.Elevation);
            Assert.Equal(0x0B, dim._flags);
            Assert.Equal("", dim.Text);
            Assert.Equal(0.0, dim.TextRotation);
            Assert.Equal(0.0, dim.HorizontalDirection);
            Assert.Equal(DwgVector.One, dim.InsertionScale);
            Assert.Equal(0.0, dim.InsertionRotation);
            Assert.Equal(DwgPoint.Origin, dim.InsertionPoint);
            Assert.Equal(new DwgPoint(6.661624958090417, 3.2580798072903328, 0.0), dim.SecondDefinitionPoint);
            Assert.Equal(new DwgPoint(8.69798377758747, 3.3077267441833236, 0.0), dim.ThirdDefinitionPoint);
            Assert.Equal(new DwgPoint(8.69798377758747, 4.673017325872081, 0.0), dim.FirstDefinitionPoint);
            Assert.Equal(0.0, dim.ExtensionLineRotation);
            Assert.Equal(0.0, dim.DimensionRotation);
            Assert.Equal(new DwgHandle(0x1D), dim.ResolveHandleReference(dim._dimStyleHandleReference));
            Assert.Equal(new DwgHandle(0x9F), dim.ResolveHandleReference(dim._anonymousBlockHandleReference));
        }

        [Fact]
        public void ReadRawDimensionAligned()
        {
            var dim = (DwgDimensionAligned)ParseRaw(
                0x6B, 0x00,                                     // length
                0x45, 0x80, 0x6E, 0xA7, 0xD8, 0x10, 0x00, 0x05, // data
                0x5B, 0x53, 0xB7, 0x92, 0xB9, 0x9A, 0xCA, 0xCA,
                0x1C, 0x81, 0x55, 0x6D, 0x19, 0x67, 0x3E, 0x90,
                0x28, 0x81, 0x05, 0xD4, 0xAC, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x2A, 0x41, 0x59,
                0xE6, 0x59, 0x20, 0x09, 0x20, 0x04, 0xE7, 0xDE,
                0x65, 0xA9, 0x1D, 0x81, 0xE8, 0x11, 0xE8, 0xB7,
                0x57, 0xAB, 0xF5, 0xB4, 0x22, 0x80, 0x6E, 0x48,
                0xCB, 0xDF, 0xEC, 0x81, 0x08, 0x20, 0x46, 0xF7,
                0x1E, 0x19, 0xC7, 0x7A, 0xE8, 0x92, 0x00, 0x60,
                0xDD, 0x30, 0x19, 0xD6, 0x34, 0x28, 0x81, 0x46,
                0x0A, 0x21, 0xF8, 0x21, 0xD4, 0x21, 0xEA, 0x23,
                0xAA, 0x35, 0xBB,
                0xEA, 0x25                                      // crc
            );
            Assert.Equal(new DwgHandle(0xBA), dim.Handle);
            Assert.Equal(DwgVector.ZAxis, dim.Extrusion);
            Assert.Equal(new DwgPoint(3.799510578531437, 5.070921712363694, 0.0), dim.TextMidpoint);
            Assert.Equal(0.0, dim.Elevation);
            Assert.Equal(0x0B, dim._flags);
            Assert.Equal("", dim.Text);
            Assert.Equal(0.0, dim.TextRotation);
            Assert.Equal(0.0, dim.HorizontalDirection);
            Assert.Equal(DwgVector.One, dim.InsertionScale);
            Assert.Equal(0.0, dim.InsertionRotation);
            Assert.Equal(DwgPoint.Origin, dim.InsertionPoint);
            Assert.Equal(new DwgPoint(4.563182066398969, 3.990372046456986, 0.0), dim.SecondDefinitionPoint);
            Assert.Equal(new DwgPoint(4.463847483238158, 4.00278376925095, 0.0), dim.ThirdDefinitionPoint);
            Assert.Equal(new DwgPoint(4.591732871013816, 5.026287266543974, 0.0), dim.FirstDefinitionPoint);
            Assert.Equal(0.0, dim.ExtensionLineRotation);
            Assert.Equal(new DwgHandle(0x1D), dim.ResolveHandleReference(dim._dimStyleHandleReference));
            Assert.Equal(new DwgHandle(0xAD), dim.ResolveHandleReference(dim._anonymousBlockHandleReference));
        }

        [Fact]
        public void ReadRawDimensionAngular3Point()
        {
            var dim = (DwgDimensionAngular3Point)ParseRaw(
                0x7B, 0x00,                                     // length
                0x45, 0xC0, 0x72, 0x63, 0xF8, 0x18, 0x00, 0x05, // data
                0x5B, 0x53, 0xDC, 0x3A, 0x57, 0xCD, 0x05, 0x40,
                0x2E, 0x80, 0xC0, 0x5E, 0xB2, 0xD6, 0x6F, 0x22,
                0x40, 0x81, 0x05, 0xD4, 0xAC, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x68, 0x08, 0xAF,
                0x7C, 0x2C, 0x5E, 0x0C, 0xA0, 0x11, 0xC0, 0xE9,
                0x18, 0x9D, 0x34, 0x04, 0x28, 0x10, 0xD2, 0xBA,
                0xAD, 0xA6, 0xB9, 0x2C, 0x3A, 0x80, 0x61, 0xCA,
                0x13, 0x3A, 0x13, 0x1C, 0x90, 0x20, 0x45, 0x65,
                0x3A, 0x06, 0x5E, 0x80, 0x38, 0xEA, 0x00, 0xD8,
                0x3B, 0x7A, 0x98, 0xA2, 0x88, 0x3A, 0x81, 0x0A,
                0x88, 0xA1, 0xBF, 0x90, 0x3F, 0xC3, 0x48, 0x00,
                0x55, 0x2D, 0xC2, 0xC7, 0x6F, 0x28, 0xFA, 0x04,
                0x60, 0xA2, 0x1F, 0x82, 0x1F, 0x42, 0x18, 0xA2,
                0x3A, 0xA3, 0x76,
                0x42, 0x38                                      // crc
            );
            Assert.Equal(new DwgHandle(0xC9), dim.Handle);
            Assert.Equal(DwgVector.ZAxis, dim.Extrusion);
            Assert.Equal(new DwgPoint(5.9067493404213405, 8.283625940930222, 0.0), dim.TextMidpoint);
            Assert.Equal(0.0, dim.Elevation);
            Assert.Equal(0x0B, dim._flags);
            Assert.Equal("", dim.Text);
            Assert.Equal(0.0, dim.TextRotation);
            Assert.Equal(0.0, dim.HorizontalDirection);
            Assert.Equal(DwgVector.One, dim.InsertionScale);
            Assert.Equal(0.0, dim.InsertionRotation);
            Assert.Equal(DwgPoint.Origin, dim.InsertionPoint);
            Assert.Equal(new DwgPoint(6.433933144322921, 8.814283498566308, 0.0), dim.FirstDefinitionPoint);
            Assert.Equal(new DwgPoint(7.396838476320547, 8.111621503553016, 0.0), dim.SecondDefinitionPoint);
            Assert.Equal(new DwgPoint(7.257632423230387, 7.316716383974704, 0.0), dim.ThirdDefinitionPoint);
            Assert.Equal(new DwgPoint(6.7485427268123495, 7.974538217801873, 0.0), dim.FourthDefinitionPoint);
            Assert.Equal(new DwgHandle(0x1D), dim.ResolveHandleReference(dim._dimStyleHandleReference));
            Assert.Equal(new DwgHandle(0xBB), dim.ResolveHandleReference(dim._anonymousBlockHandleReference));
        }

        [Fact]
        public void ReadRawDimensionRadius()
        {
            var dim = (DwgDimensionRadius)ParseRaw(
                0x71, 0x00,                                     // length
                0x46, 0x40, 0x75, 0x51, 0x45, 0x11, 0x10, 0x00, // data
                0x60, 0x01, 0xE4, 0x45, 0x35, 0x45, 0x94, 0xC4,
                0x50, 0x20, 0x04, 0x62, 0x00, 0x14, 0x60, 0x10,
                0x00, 0x20, 0x18, 0x5E, 0x06, 0x00, 0x01, 0x56,
                0xD4, 0xBE, 0xB8, 0xAD, 0x7A, 0xBB, 0x82, 0x11,
                0x20, 0x48, 0x89, 0x3F, 0xDF, 0xD9, 0xA0, 0x0F,
                0xA0, 0x41, 0x55, 0x2B, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x0A, 0xA8, 0xA1, 0xBF,
                0x90, 0x3F, 0xC3, 0x48, 0x00, 0x55, 0x2D, 0xC2,
                0xC7, 0x6F, 0x28, 0xFA, 0x04, 0x27, 0xF9, 0x9E,
                0x65, 0xFB, 0x50, 0x0E, 0xA0, 0x07, 0xFF, 0xE7,
                0x46, 0x14, 0xF3, 0x63, 0xE8, 0x14, 0x60, 0xA2,
                0x1F, 0x82, 0x19, 0x42, 0x18, 0xA2, 0x3A, 0xA3,
                0x94,
                0xEA, 0x1E                                      // crc
            );
            Assert.Equal(new DwgHandle(0xD5), dim.Handle);
            Assert.Equal(DwgVector.ZAxis, dim.Extrusion);
            Assert.Equal(new DwgPoint(9.008727710047191, 7.813185684328929, 0.0), dim.TextMidpoint);
            Assert.Equal(0.0, dim.Elevation);
            Assert.Equal(0x0A, dim._flags);
            Assert.Equal("", dim.Text);
            Assert.Equal(0.0, dim.TextRotation);
            Assert.Equal(0.0, dim.HorizontalDirection);
            Assert.Equal(DwgVector.One, dim.InsertionScale);
            Assert.Equal(0.0, dim.InsertionRotation);
            Assert.Equal(DwgPoint.Origin, dim.InsertionPoint);
            Assert.Equal(new DwgPoint(6.74854272681235, 7.974538217801873, 0.0), dim.FirstDefinitionPoint);
            Assert.Equal(new DwgPoint(7.407191444010848, 7.902004960633177, 0.0), dim.SecondDefinitionPoint);
            Assert.Equal(0.0, dim.LeaderLength);
            Assert.Equal(new DwgHandle(0x1D), dim.ResolveHandleReference(dim._dimStyleHandleReference));
            Assert.Equal(new DwgHandle(0xCA), dim.ResolveHandleReference(dim._anonymousBlockHandleReference));

            // verify xdata
            Assert.Equal(1, dim._xdataMap.Keys.Count);
            var items = dim._xdataMap[new DwgHandle(0x11)];
            Assert.Equal(2, items.Count);
            Assert.Equal("DSTYLE", ((DwgXDataString)items[0]).Value);
            var innerItems = (DwgXDataItemList)items[1];
            Assert.Equal(2, innerItems.Count);
            Assert.Equal(288, ((DwgXDataShort)innerItems[0]).Value);
            Assert.Equal(1, ((DwgXDataShort)innerItems[1]).Value);
        }

        [Fact]
        public void ReadRawDimensionDiameter()
        {
            var dim = (DwgDimensionDiameter)ParseRaw(
                0x70, 0x00,                                     // length
                0x46, 0x80, 0x78, 0x51, 0x45, 0x11, 0x10, 0x00, // data
                0x60, 0x01, 0xE4, 0x45, 0x35, 0x45, 0x94, 0xC4,
                0x50, 0x20, 0x04, 0x62, 0x00, 0x14, 0x60, 0x10,
                0x00, 0x20, 0x18, 0x5E, 0x06, 0x00, 0x01, 0x56,
                0xD4, 0xAE, 0x72, 0x3A, 0xF7, 0x9A, 0xB2, 0x10,
                0xA0, 0x4A, 0x92, 0xA4, 0x03, 0x41, 0xDC, 0x0E,
                0x20, 0x41, 0x55, 0x2B, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x1B, 0x6E, 0xED, 0x97,
                0x4E, 0x85, 0xE3, 0xA8, 0x06, 0x3F, 0xD6, 0x3A,
                0xB1, 0x4B, 0x40, 0xF2, 0x04, 0x65, 0x89, 0x57,
                0x1E, 0xC7, 0xE6, 0x8C, 0x20, 0x14, 0x94, 0xEA,
                0x95, 0xBB, 0x16, 0x24, 0x08, 0x14, 0x60, 0xA2,
                0x1F, 0x82, 0x18, 0xC0, 0xA2, 0x3A, 0xA3, 0xAD,
                0x37, 0xB4                                      // crc
            );
            Assert.Equal(new DwgHandle(0xE1), dim.Handle);
            Assert.Equal(DwgVector.ZAxis, dim.Extrusion);
            Assert.Equal(new DwgPoint(8.695723994389645, 7.180187318948261, 0.0), dim.TextMidpoint);
            Assert.Equal(0.0, dim.Elevation);
            Assert.Equal(0x0A, dim._flags);
            Assert.Equal("", dim.Text);
            Assert.Equal(0.0, dim.TextRotation);
            Assert.Equal(0.0, dim.HorizontalDirection);
            Assert.Equal(DwgVector.One, dim.InsertionScale);
            Assert.Equal(0.0, dim.InsertionRotation);
            Assert.Equal(DwgPoint.Origin, dim.InsertionPoint);
            Assert.Equal(new DwgPoint(7.296343720312474, 7.601720195728098, 0.0), dim.FirstDefinitionPoint);
            Assert.Equal(new DwgPoint(6.200741733312223, 8.347356239875644, 0.0), dim.SecondDefinitionPoint);
            Assert.Equal(0.0, dim.LeaderLength);
            Assert.Equal(new DwgHandle(0x1D), dim.ResolveHandleReference(dim._dimStyleHandleReference));
            Assert.Equal(new DwgHandle(0xD6), dim.ResolveHandleReference(dim._anonymousBlockHandleReference));

            // verify xdata
            Assert.Equal(1, dim._xdataMap.Keys.Count);
            var items = dim._xdataMap[new DwgHandle(0x11)];
            Assert.Equal(2, items.Count);
            Assert.Equal("DSTYLE", ((DwgXDataString)items[0]).Value);
            var innerItems = (DwgXDataItemList)items[1];
            Assert.Equal(2, innerItems.Count);
            Assert.Equal(288, ((DwgXDataShort)innerItems[0]).Value);
            Assert.Equal(1, ((DwgXDataShort)innerItems[1]).Value);
        }

        [Fact]
        public void ReadRawLocation()
        {
            var loc = (DwgLocation)ParseRaw(
                0x23, 0x00,                                     // length
                0x46, 0xC0, 0x74, 0xA6, 0xC8, 0x00, 0x00, 0x01, // data
                0x33, 0x09, 0xFE, 0x67, 0x99, 0x7E, 0xD4, 0x03,
                0xA8, 0x01, 0xFF, 0xF9, 0xD1, 0x85, 0x3C, 0xD8,
                0xFA, 0x05, 0x53, 0x60, 0x84, 0x18, 0x28, 0xCC,
                0xA8, 0x89, 0x86,
                0x09, 0xDF                                      // crc
            );
            Assert.Equal(new DwgHandle(0xD2), loc.Handle);
            Assert.Equal(new DwgPoint(7.407191444010848, 7.902004960633177, 0.0), loc.Point);
            Assert.Equal(0.0, loc.Thickness);
            Assert.Equal(DwgVector.ZAxis, loc.Extrusion);
            Assert.Equal(0.0, loc.XAxisAngle);
        }

        [Fact]
        public void ReadRawFace3D()
        {
            var face = (DwgFace3D)ParseRaw(
                0x50, 0x00,                                     // length
                0x47, 0x00, 0x78, 0xE3, 0x18, 0x10, 0x00, 0x05, // data
                0x7B, 0x06, 0x54, 0xB1, 0x62, 0xD9, 0xBA, 0xE4,
                0x28, 0x00, 0x02, 0x01, 0x84, 0xE7, 0x8E, 0x80,
                0x12, 0x04, 0x4F, 0x73, 0xC2, 0x29, 0x98, 0x53,
                0x12, 0x20, 0x16, 0x8C, 0x3C, 0xB6, 0xE3, 0x69,
                0xE2, 0x08, 0x10, 0x14, 0x77, 0x8D, 0x5D, 0xFA,
                0x52, 0x4C, 0x80, 0x50, 0x0B, 0x36, 0xA4, 0x30,
                0xF0, 0xFF, 0x1F, 0xC5, 0x51, 0x57, 0x68, 0x85,
                0x48, 0x51, 0x12, 0x00, 0x40, 0x84, 0xCA, 0xFB,
                0xAF, 0xDF, 0xEC, 0x7F, 0x46, 0x0A, 0x21, 0xFA,
                0x1A, 0xA6                                      // crc
            );
            Assert.Equal(new DwgHandle(0xE3), face.Handle);
            Assert.Equal(new DwgPoint(8.92148657278685, 2.3520233347153408, 0.0), face.FirstCorner);
            Assert.Equal(new DwgPoint(10.324587450002408, 4.07725417459044, 0.0), face.SecondCorner);
            Assert.Equal(new DwgPoint(11.082013570497583, 1.9300244168420377, 0.0), face.ThirdCorner);
            Assert.Equal(new DwgPoint(9.020821122122772, 1.4335551164878169, 0.0), face.FourthCorner);
            Assert.Equal(0, face.InvisibilityFlags);
        }

        [Fact]
        public void ReadRawPolylinePFace()
        {
            var pface = (DwgPolylinePFace)ParseRaw(
                0x19, 0x00,                                     // length
                0x47, 0x40, 0x55, 0x62, 0xE8, 0x00, 0x00, 0x05, // data
                0x5B, 0x20, 0x88, 0x19, 0x82, 0x88, 0x7E, 0x08,
                0x4D, 0x08, 0x4A, 0x0A, 0xB2, 0x0A, 0xE1, 0x8A,
                0xE8,
                0xD7, 0x3E                                      // crc
            );
            Assert.Equal(new DwgHandle(0x55), pface.Handle);
            Assert.Equal(4, pface._vertexCount);
            Assert.Equal(3, pface._faceCount);
            Assert.Equal(new DwgHandle(0x56), pface.ResolveHandleReference(pface._firstVertexHandleReference));
            Assert.Equal(new DwgHandle(0x5C), pface.ResolveHandleReference(pface._lastVertexHandleReference));
            Assert.Equal(new DwgHandle(0x5D), pface.ResolveHandleReference(pface._seqEndHandleReference));
        }

        [Fact]
        public void ReadRawPolylineMesh()
        {
            var mesh = (DwgPolylineMesh)ParseRaw(
                0x1A, 0x00,                                     // length
                0x47, 0x80, 0x59, 0xA3, 0x68, 0x00, 0x00, 0x05, // data
                0x5B, 0x22, 0x32, 0x0C, 0x83, 0xD1, 0x82, 0x88,
                0x7C, 0x05, 0x09, 0x62, 0x0B, 0x3A, 0x0C, 0x81,
                0x8C, 0x8C,
                0x3C, 0xE7                                      // crc
            );
            Assert.Equal(new DwgHandle(0x66), mesh.Handle);
            Assert.Equal(0x11, mesh.Flags);
            Assert.Equal(DwgCurveType.None, mesh.CurveType);
            Assert.Equal(6, mesh._mVertexCount);
            Assert.Equal(7, mesh._nVertexCount);
            Assert.Equal(0, mesh._mDensity);
            Assert.Equal(0, mesh._nDensity);
            Assert.Equal(new DwgHandle(0x67), mesh.ResolveHandleReference(mesh._firstVertexHandleReference));
            Assert.Equal(new DwgHandle(0x90), mesh.ResolveHandleReference(mesh._lastVertexHandleReference));
            Assert.Equal(new DwgHandle(0x91), mesh.ResolveHandleReference(mesh._seqEndHandleReference));
        }

        [Fact]
        public void ReadRawSolid()
        {
            var solid = (DwgSolid)ParseRaw(
                0x52, 0x00,                                     // length
                0x47, 0xC0, 0x73, 0xE2, 0x98, 0x10, 0x00, 0x01, // data
                0x33, 0x50, 0xA8, 0xBE, 0x18, 0x24, 0x52, 0xD8,
                0xF2, 0x07, 0x52, 0xC3, 0x01, 0x40, 0x1D, 0x30,
                0xFA, 0x00, 0xFF, 0x31, 0x0A, 0x96, 0x82, 0xA0,
                0xF2, 0x06, 0x8B, 0xFA, 0x70, 0x47, 0x8B, 0x40,
                0xFA, 0x02, 0x7F, 0x99, 0xE6, 0x5F, 0xB5, 0x00,
                0xEA, 0x01, 0xFF, 0xF9, 0xD1, 0x85, 0x3C, 0xD8,
                0xFA, 0x02, 0x7F, 0x99, 0xE6, 0x5F, 0xB5, 0x00,
                0xEA, 0x01, 0xFF, 0xF9, 0xD1, 0x85, 0x3C, 0xD8,
                0xFA, 0x05, 0x38, 0x20, 0xA6, 0x0A, 0x21, 0xEA,
                0x22, 0x6A,
                0x18, 0x03                                      // crc
            );
            Assert.Equal(new DwgHandle(0xCF), solid.Handle);
            Assert.Equal(0.0, solid.Thickness);
            Assert.Equal(0.0, solid.Elevation);
            Assert.Equal(new DwgPoint(7.589393686683043, 7.912121420746397, 0.0), solid.FirstCorner);
            Assert.Equal(new DwgPoint(7.582825930899644, 7.852481965819565, 0.0), solid.SecondCorner);
            Assert.Equal(new DwgPoint(7.407191444010848, 7.902004960633177, 0.0), solid.ThirdCorner);
            Assert.Equal(new DwgPoint(7.407191444010848, 7.902004960633177, 0.0), solid.FourthCorner);
            Assert.Equal(new DwgVector(0.0, 0.0, 1.0), solid.Extrusion);
        }

        [Fact]
        public void ReadRawTrace()
        {
            var trace = (DwgTrace)ParseRaw(
                0x51, 0x00,                                     // length
                0x48, 0x00, 0x79, 0xE2, 0x98, 0x10, 0x00, 0x05, // data
                0x5B, 0x53, 0x70, 0xDA, 0xA0, 0xAD, 0xEE, 0xC1,
                0x42, 0x05, 0xBA, 0xE0, 0x2A, 0xDA, 0xA9, 0x60,
                0x02, 0x05, 0x75, 0x29, 0xDE, 0x3E, 0xFF, 0x89,
                0x42, 0x03, 0x4E, 0x20, 0xB3, 0x8F, 0x50, 0xC0,
                0x02, 0x00, 0x4B, 0x2A, 0x12, 0x65, 0x70, 0xA9,
                0x52, 0x04, 0x9E, 0x9B, 0xA5, 0x92, 0xBF, 0x40,
                0xA2, 0x06, 0x1D, 0x16, 0xC2, 0xA5, 0x61, 0x81,
                0x52, 0x02, 0x6F, 0x4E, 0x85, 0xD6, 0xE7, 0x88,
                0xA2, 0x05, 0x26, 0x0A, 0x21, 0xF8, 0x20, 0x6C,
                0x1A,
                0x7E, 0xC2                                      // crc
            );
            Assert.Equal(new DwgHandle(0xE7), trace.Handle);
            Assert.Equal(0.0, trace.Thickness);
            Assert.Equal(0.0, trace.Elevation);
            Assert.Equal(new DwgPoint(12.423317591206537, 2.021647177792683, 0.0), trace.FirstCorner);
            Assert.Equal(new DwgPoint(12.47241041758647, 2.012165912158299, 0.0), trace.SecondCorner);
            Assert.Equal(new DwgPoint(13.042345412354296, 5.226897037868997, 0.0), trace.ThirdCorner);
            Assert.Equal(new DwgPoint(13.09506478447418, 5.2361935796746595, 0.0), trace.FourthCorner);
            Assert.Equal(new DwgVector(0.0, 0.0, 1.0), trace.Extrusion);
        }

        [Fact]
        public void ReadRawShape()
        {
            var shape = (DwgShape)ParseRaw(
                0x26, 0x00,                                     // length
                0x48, 0x40, 0x7D, 0x67, 0x48, 0x00, 0x00, 0x01, // data
                0x5B, 0x14, 0xAF, 0x3D, 0x96, 0x39, 0x59, 0xA1,
                0x48, 0x04, 0x20, 0xD5, 0x14, 0x35, 0x41, 0x08,
                0x8A, 0x04, 0xCD, 0x32, 0xF4, 0xC0, 0x18, 0x28,
                0x87, 0xA0, 0x30, 0x28, 0xF9, 0xED,
                0x38, 0x74                                      // crc
            );
            Assert.Equal(new DwgHandle(0xF5), shape.Handle);
            Assert.Equal(new DwgPoint(3.350484266308927, 4.282869437831895, 0.0), shape.InsertionPoint);
            Assert.Equal(1.0, shape.Scale);
            Assert.Equal(0.0, shape.Rotation);
            Assert.Equal(1.0, shape.WidthFactor);
            Assert.Equal(0.0, shape.Oblique);
            Assert.Equal(0.0, shape.Thickness);
            Assert.Equal(151, shape.ShapeNumber);
            Assert.Equal(new DwgVector(0.0, 0.0, 1.0), shape.Extrusion);
        }

        [Fact]
        public void ReadRawViewPortEntity()
        {
            var vp = (DwgViewPortEntity)ParseRaw(
                0x17, 0x01,                                     // length
                0x48, 0x80, 0x80, 0x49, 0x9D, 0xF5, 0x11, 0x10, // data
                0x00, 0x50, 0x01, 0xE4, 0xD5, 0x64, 0x94, 0x55,
                0x70, 0x20, 0x04, 0x61, 0x00, 0x00, 0xA0, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA0,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x03, 0xF2,
                0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x02, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02,
                0x24, 0x02, 0x87, 0x89, 0x21, 0xA6, 0x5A, 0xCA,
                0x21, 0xA4, 0x02, 0x80, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x01, 0x24, 0x02, 0x80, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x04, 0x94, 0x02, 0x80, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x80, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x60,
                0x00, 0x04, 0x66, 0x40, 0x04, 0x60, 0x10, 0x04,
                0x60, 0x10, 0x04, 0x60, 0x00, 0x04, 0x60, 0x00,
                0x04, 0x60, 0x00, 0x04, 0x60, 0x00, 0x02, 0x80,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02,
                0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x02, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x02, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x0E, 0x03, 0xF2, 0x80, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x0E, 0x03, 0xF2, 0x80, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x0E, 0x03, 0xF2, 0x80, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x0E, 0x03, 0xF4, 0x60, 0x00,
                0x00, 0x20, 0x00, 0x20, 0x10, 0x20, 0x18, 0xDA,
                0x10, 0x00, 0x00, 0xD6, 0xC3, 0xC4, 0x90, 0xD3,
                0x2D, 0x65, 0x10, 0xD2, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x24, 0x81, 0x0F, 0x12, 0x43,
                0x4C, 0xB5, 0x94, 0x45, 0x48, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x01, 0x12, 0x01, 0x82, 0x88,
                0x7A, 0x05, 0x08, 0x12, 0x90, 0x09, 0x28,
                0x6C, 0x19                                      // crc
            );
            Assert.Equal(new DwgHandle(0x126), vp.Handle);
            Assert.Equal(new DwgPoint(6.65886075949367, 4.5, 0.0), vp.Center);
            Assert.Equal(13.317721518987341, vp.Width);
            Assert.Equal(9.0, vp.Height);
            Assert.Equal(new DwgHandle(0x125), vp.ResolveHandleReference(vp._viewPortEntityHeaderHandleReference));

        }

        [Fact]
        public void ReadRawEllipse()
        {
            var el = (DwgEllipse)ParseRaw(
                0x4C, 0x00,                                     // length
                0x48, 0xC0, 0x80, 0x48, 0xA1, 0x48, 0x10, 0x00, // data
                0x05, 0x5B, 0x0C, 0x0A, 0x03, 0x29, 0x8A, 0xE7,
                0x42, 0x48, 0x01, 0xF0, 0x9F, 0xBC, 0x53, 0x10,
                0x40, 0xDA, 0x04, 0x51, 0x23, 0xD0, 0xF1, 0xD6,
                0xAF, 0x7B, 0x9F, 0x9A, 0x89, 0x15, 0xEA, 0x36,
                0xB2, 0xDD, 0x17, 0xF5, 0x00, 0x20, 0x00, 0x00,
                0x00, 0x00, 0x1E, 0x07, 0xE5, 0xD2, 0xA4, 0x7D,
                0xB0, 0x4C, 0x5E, 0xF9, 0xFC, 0x0C, 0x16, 0xA2,
                0x2A, 0x7D, 0x90, 0x8C, 0xA0, 0x18, 0x28, 0x87,
                0xE0, 0x83, 0xA0, 0x69,
                0xED, 0x08                                      // crc
            );
            Assert.Equal(new DwgHandle(0x0122), el.Handle);
            Assert.Equal(new DwgPoint(4.556973637623827, 6.758188411073943, 0.0), el.Center);
            Assert.Equal(new DwgVector(1.4606150523945662, -0.7683970065502543, 0.0), el.MajorAxis);
            Assert.Equal(new DwgVector(0.0, 0.0, 1.0000000000000002), el.Extrusion);
            Assert.Equal(0.4928612025081055, el.MinorAxisRatio);
            Assert.Equal(0.0, el.StartAngle);
            Assert.Equal(6.283185307179586, el.EndAngle);
        }

        [Fact]
        public void ReadRawSpline1()
        {
            var spline = (DwgSpline)ParseRaw(
                0x61, 0x00,                                     // length
                0x49, 0x00, 0x80, 0x40, 0x66, 0xA8, 0x10, 0x00, // data
                0x05, 0x5B, 0x20, 0x48, 0x19, 0x77, 0x7B, 0xAF,
                0xB3, 0xBE, 0xF9, 0xB6, 0x7B, 0x55, 0x48, 0x21,
                0xD1, 0xF6, 0xEC, 0x49, 0x3D, 0x16, 0x1C, 0x80,
                0x60, 0x3C, 0x07, 0x63, 0x43, 0x16, 0xF8, 0x9F,
                0xC0, 0xE4, 0x4E, 0x53, 0x64, 0xCA, 0x30, 0xB2,
                0x01, 0xF0, 0x33, 0x3C, 0x1C, 0xA7, 0xC2, 0x0E,
                0x81, 0x01, 0x85, 0x80, 0x9A, 0xFE, 0x6F, 0x63,
                0x88, 0x02, 0x07, 0x89, 0xBE, 0x3C, 0x1B, 0x4F,
                0x51, 0xFC, 0x5F, 0x51, 0x14, 0xFA, 0x2F, 0xCF,
                0x94, 0x20, 0x04, 0x18, 0xCB, 0x8B, 0xBB, 0xC6,
                0x9D, 0x67, 0xF1, 0x82, 0x88, 0x7E, 0x08, 0x13,
                0x05,
                0x99, 0xF5                                      // crc
            );
            Assert.Equal(new DwgHandle(0x0101), spline.Handle);
            Assert.Equal(DwgSplineType.FitPointsOnly, spline.SplineType);
            Assert.Equal(3, spline.Degree);
            Assert.Equal(1.0e-10, spline.FitTolerance);
            Assert.Equal(new DwgVector(0.0, 0.0, 0.0), spline.StartTangentVector);
            Assert.Equal(new DwgVector(0.0, 0.0, 0.0), spline.EndTangentVector);
            Assert.Equal(4, spline.FitPoints.Count);
            Assert.Equal(new DwgPoint(3.8181727265177763, 1.0736148583014682, 0.0), spline.FitPoints[0]);
            Assert.Equal(new DwgPoint(5.568944644796172, 2.9850217000960093, 0.0), spline.FitPoints[1]);
            Assert.Equal(new DwgPoint(7.121047362928049, 0.8253802195536437, 0.0), spline.FitPoints[2]);
            Assert.Equal(new DwgPoint(12.311278944094166, 0.8502036880001391, 0.0), spline.FitPoints[3]);
            Assert.False(spline.IsRational);
            Assert.False(spline.IsClosed);
            Assert.False(spline.IsPeriodic);
            Assert.Equal(0.0, spline.KnotTolerance);
            Assert.Equal(0.0, spline.ControlTolerance);
            Assert.Empty(spline.KnotValues);
            Assert.Empty(spline.ControlPoints);
        }

        [Fact]
        public void ReadRawSpline2()
        {
            var spline = (DwgSpline)ParseRaw(
                0xBB, 0x00,                                     // length
                0x49, 0x00, 0x80, 0x40, 0xA5, 0xC8, 0x28, 0x00, // data
                0x05, 0x7B, 0x20, 0x28, 0x18, 0x12, 0x2B, 0xEF,
                0x26, 0xBC, 0xB5, 0xDE, 0x8F, 0x84, 0x8A, 0xFB,
                0xC9, 0xAF, 0x2D, 0x77, 0xA3, 0xE4, 0x29, 0x06,
                0x55, 0x01, 0xE2, 0x91, 0xA5, 0xD0, 0x17, 0x80,
                0x88, 0x04, 0x31, 0x35, 0xFD, 0x44, 0xD0, 0x08,
                0xAA, 0x01, 0x79, 0x09, 0xBE, 0x48, 0x77, 0xC4,
                0x48, 0x80, 0x5E, 0x42, 0x6F, 0x92, 0x1D, 0xF1,
                0x12, 0x20, 0x17, 0x90, 0x9B, 0xE4, 0x87, 0x7C,
                0x44, 0x88, 0x05, 0xE4, 0x26, 0xF9, 0x21, 0xDF,
                0x11, 0x22, 0x00, 0x51, 0x81, 0x5C, 0xA9, 0x30,
                0xEA, 0x18, 0x80, 0x40, 0x1B, 0x4B, 0xCF, 0x66,
                0xF3, 0x7D, 0x9F, 0xC4, 0x63, 0x6D, 0xAF, 0x9B,
                0x7D, 0xA8, 0x82, 0x00, 0x51, 0x75, 0x80, 0x5C,
                0xB0, 0x1C, 0x0C, 0x81, 0x09, 0xD0, 0x81, 0x4C,
                0x07, 0xB7, 0x62, 0xA8, 0x05, 0x58, 0xF7, 0xA1,
                0x59, 0x01, 0xE8, 0x9A, 0x04, 0x5B, 0x36, 0x58,
                0x8C, 0x6B, 0x16, 0x0E, 0x20, 0x1F, 0xC2, 0x20,
                0x5C, 0x89, 0xE6, 0xDA, 0x97, 0xF1, 0x0C, 0x22,
                0xB6, 0x2B, 0x1A, 0x3A, 0x48, 0x80, 0x23, 0x05,
                0x1E, 0xF2, 0x8C, 0x22, 0x74, 0x9F, 0xC6, 0x74,
                0x99, 0xBC, 0x06, 0xF0, 0xC9, 0x42, 0x01, 0xA0,
                0x40, 0x6E, 0x0F, 0x6C, 0xA7, 0xF0, 0x7F, 0x18,
                0x28, 0x87, 0xD7,
                0xE3, 0xF3                                      // crc
            );
            Assert.Equal(new DwgHandle(0x0102), spline.Handle);
            Assert.Equal(DwgSplineType.ControlAndKnotsOnly, spline.SplineType);
            Assert.Equal(3, spline.Degree);
            Assert.Equal(0.0, spline.FitTolerance);
            Assert.Equal(new DwgVector(1.0, 0.0, 0.0), spline.StartTangentVector);
            Assert.Equal(new DwgVector(1.0, 0.0, 0.0), spline.EndTangentVector);
            Assert.Empty(spline.FitPoints);
            Assert.False(spline.IsRational);
            Assert.False(spline.IsClosed);
            Assert.False(spline.IsPeriodic);
            Assert.Equal(1.0e-7, spline.KnotTolerance);
            Assert.Equal(1.0e-7, spline.ControlTolerance);
            Assert.Equal(10, spline.KnotValues.Count);
            Assert.Equal(0.0, spline.KnotValues[0]);
            Assert.Equal(0.0, spline.KnotValues[1]);
            Assert.Equal(0.0, spline.KnotValues[2]);
            Assert.Equal(0.0, spline.KnotValues[3]);
            Assert.Equal(2.592041362457628, spline.KnotValues[4]);
            Assert.Equal(5.2515665404004093, spline.KnotValues[5]);
            Assert.Equal(10.441857483181927, spline.KnotValues[6]);
            Assert.Equal(10.441857483181927, spline.KnotValues[7]);
            Assert.Equal(10.441857483181927, spline.KnotValues[8]);
            Assert.Equal(10.441857483181927, spline.KnotValues[9]);
            Assert.Equal(6, spline.ControlPoints.Count);
            Assert.Equal(new DwgPoint(3.5574194541768698, 1.7438484377811676, 0.0), spline.ControlPoints[0].Point);
            Assert.Equal(1.0, spline.ControlPoints[0].Weight);
            Assert.Equal(new DwgPoint(4.177184875439149, 2.757004132515494, 0.0), spline.ControlPoints[1].Point);
            Assert.Equal(1.0, spline.ControlPoints[1].Weight);
            Assert.Equal(new DwgPoint(5.432851315156837, 4.809693037772074, 0.0), spline.ControlPoints[2].Point);
            Assert.Equal(1.0, spline.ControlPoints[2].Weight);
            Assert.Equal(new DwgPoint(7.043785463170186, -0.3158149458543277, 0.0), spline.ControlPoints[3].Point);
            Assert.Equal(1.0, spline.ControlPoints[3].Weight);
            Assert.Equal(new DwgPoint(10.05771700607944, 0.7895626523316615, 0.0), spline.ControlPoints[4].Point);
            Assert.Equal(1.0, spline.ControlPoints[4].Weight);
            Assert.Equal(new DwgPoint(12.05052567175326, 1.5204372674798385, 0.0), spline.ControlPoints[5].Point);
            Assert.Equal(1.0, spline.ControlPoints[5].Weight);
            Assert.Empty(spline.FitPoints);
        }

        [Fact]
        public void ReadRawRegion()
        {
            var region = (DwgRegion)ParseRaw(
                0x2D, 0x02,                                     // length
                0x49, 0x40, 0x80, 0x40, 0xE2, 0x48, 0x88, 0x00, // data
                0x05, 0x7B, 0x28, 0x08, 0x0C, 0x04, 0x00, 0x00,
                0xDC, 0xDE, 0xD2, 0x40, 0xDC, 0xDC, 0x40, 0xDC,
                0x40, 0xDE, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40,
                0x40, 0x40, 0x40, 0x40, 0x1A, 0x14, 0x7A, 0x60,
                0x76, 0x4C, 0x40, 0xF6, 0xE4, 0xDC, 0x40, 0xF6,
                0xDC, 0x40, 0xF6, 0xE4, 0xDC, 0x40, 0xF6, 0xE4,
                0xDC, 0x40, 0xF8, 0x1A, 0x14, 0x66, 0x54, 0x64,
                0x5E, 0x40, 0xF6, 0xE4, 0xDC, 0x40, 0xF6, 0xE4,
                0xDC, 0x40, 0xF6, 0xDA, 0x40, 0xF6, 0xDE, 0x40,
                0xF8, 0x1A, 0x14, 0x58, 0x6E, 0x74, 0x66, 0x66,
                0x40, 0xF6, 0xE4, 0xDC, 0x40, 0xF6, 0xE4, 0xDC,
                0x40, 0xF6, 0xE4, 0xDC, 0x40, 0xF6, 0xD8, 0x40,
                0xF6, 0xDC, 0x40, 0xF8, 0x1A, 0x14, 0x72, 0x7C,
                0x78, 0x74, 0x40, 0xF6, 0xE4, 0xDC, 0x40, 0xF6,
                0xE4, 0xDC, 0x40, 0xF6, 0xD6, 0x40, 0xF6, 0xDA,
                0x40, 0xF6, 0xE4, 0xDC, 0x40, 0xF6, 0xD4, 0x40,
                0x72, 0x60, 0x5A, 0x50, 0x7C, 0x5A, 0x76, 0x40,
                0x76, 0x60, 0x54, 0x7A, 0x66, 0x74, 0x40, 0x60,
                0x54, 0x56, 0x40, 0xF8, 0x1A, 0x14, 0x66, 0x60,
                0x60, 0x5E, 0x40, 0xF6, 0xE4, 0xDC, 0x40, 0xF6,
                0xE4, 0xDC, 0x40, 0xF6, 0xD2, 0x40, 0xF6, 0xD8,
                0x40, 0xF8, 0x1A, 0x14, 0x5E, 0x66, 0x7C, 0x62,
                0x74, 0xE4, 0x58, 0x54, 0x5A, 0x72, 0x7C, 0x78,
                0x74, 0x40, 0xF6, 0xE4, 0xDC, 0x40, 0xCE, 0xE2,
                0xDC, 0xDE, 0xDC, 0xCC, 0xD0, 0xD2, 0xD8, 0xDC,
                0xDA, 0xD6, 0xD6, 0xD0, 0xD6, 0xCC, 0xCC, 0xD4,
                0x40, 0xDC, 0xE2, 0xCE, 0xD0, 0xD6, 0xDC, 0xD0,
                0xDC, 0xD2, 0xDC, 0xCE, 0xD4, 0xD4, 0xDA, 0xDE,
                0xD2, 0xDC, 0xD2, 0x40, 0xDE, 0x40, 0xDE, 0x40,
                0xDE, 0x40, 0xDC, 0x40, 0xDC, 0x40, 0xDE, 0x40,
                0xDE, 0x40, 0xDE, 0x40, 0xAC, 0x40, 0xAC, 0x40,
                0xAC, 0x40, 0xAC, 0x40, 0xF8, 0x1A, 0x14, 0x78,
                0x60, 0x74, 0x76, 0x70, 0x74, 0x40, 0xF6, 0xE4,
                0xDC, 0x40, 0xF6, 0xD2, 0x40, 0xF6, 0xD2, 0x40,
                0xF6, 0xE4, 0xDC, 0x40, 0xF6, 0xD0, 0x40, 0xDE,
                0x40, 0xF6, 0xD6, 0x40, 0xF6, 0xE4, 0xDC, 0x40,
                0xF8, 0x1A, 0x14, 0x74, 0x76, 0x70, 0x74, 0x40,
                0xF6, 0xE4, 0xDC, 0x40, 0xF6, 0xCE, 0x40, 0xF6,
                0xCE, 0x40, 0xF6, 0xD2, 0x40, 0xF6, 0xCC, 0x40,
                0xDE, 0x40, 0xF8, 0x1A, 0x14, 0x52, 0x74, 0x5A,
                0x56, 0x74, 0x4E, 0x40, 0xF6, 0xE4, 0xDC, 0x40,
                0xF6, 0xD0, 0x40, 0xF6, 0xDC, 0xDE, 0x40, 0xF8,
                0x1A, 0x14, 0x74, 0x66, 0x66, 0x6C, 0x5E, 0x58,
                0x74, 0xE4, 0x78, 0x54, 0x5A, 0x52, 0x74, 0x40,
                0xF6, 0xE4, 0xDC, 0x40, 0xCE, 0xE2, 0xDC, 0xDE,
                0xDC, 0xCC, 0xD0, 0xD2, 0xD8, 0xDC, 0xDA, 0xD6,
                0xD6, 0xD0, 0xD4, 0xDE, 0xDC, 0xD8, 0x40, 0xDC,
                0xE2, 0xCE, 0xD0, 0xD6, 0xDC, 0xD0, 0xDC, 0xD2,
                0xDC, 0xCE, 0xD4, 0xD4, 0xDA, 0xDE, 0xD2, 0xD8,
                0xD6, 0x40, 0xDE, 0x40, 0xDE, 0x40, 0xDE, 0x40,
                0xDC, 0x40, 0xDE, 0xE2, 0xD2, 0xD8, 0xD0, 0xD4,
                0xDE, 0xD2, 0xD8, 0xCE, 0xDA, 0xD2, 0xD0, 0xD4,
                0xD6, 0xDA, 0xCE, 0xDC, 0xD6, 0x40, 0xE4, 0xDE,
                0xE2, 0xCC, 0xDA, 0xDA, 0xD0, 0xDA, 0xD0, 0xCC,
                0xDE, 0xDA, 0xD2, 0xD6, 0xD8, 0xD6, 0xD8, 0xD0,
                0xDA, 0xCC, 0x40, 0xDE, 0x40, 0xDE, 0xE2, 0xD6,
                0xD4, 0xD0, 0xD4, 0xDA, 0xD4, 0xCC, 0xD4, 0xDC,
                0xD8, 0xD0, 0xCC, 0xDC, 0xDA, 0xD8, 0xD4, 0xD4,
                0x40, 0xAC, 0x40, 0xAC, 0x40, 0xF8, 0x1A, 0x14,
                0x5E, 0x60, 0x6C, 0x62, 0x56, 0x40, 0xF6, 0xE4,
                0xDC, 0x40, 0xCE, 0xE2, 0xD0, 0xD8, 0xCC, 0xD6,
                0xCE, 0xDA, 0xD2, 0xCC, 0xD4, 0xDC, 0xDA, 0xDA,
                0xCC, 0xDA, 0xCE, 0xD0, 0x40, 0xDE, 0xE2, 0xCC,
                0xD4, 0xDC, 0xD6, 0xD6, 0xD8, 0xD0, 0xDC, 0xD4,
                0xCC, 0xDE, 0xCE, 0xD2, 0xDA, 0xD2, 0xDE, 0xCC,
                0x40, 0xDE, 0x40, 0xF8, 0x1A, 0x15, 0x63, 0xF6,
                0xD9, 0xE9, 0xE9, 0xB1, 0xA1, 0x02, 0x00, 0x50,
                0xB8, 0x18, 0xC3, 0x37, 0xF9, 0xFA, 0x7F, 0x20,
                0x9A, 0x98, 0x28, 0x87, 0x80,
                0x07, 0x33                                      // crc
            );
            Assert.Equal(new DwgHandle(0x0103), region.Handle);
            Assert.Equal(518, region.RawData.Count);
            Assert.Equal(19, region.TrailingData.Count);
            Assert.Equal(0, region.FinalBitCount);
            Assert.Equal(0, region.FinalByte);
        }

        [Fact]
        public void ReadRawSolid3D()
        {
            var solid = (DwgSolid3D)ParseRaw(
                0x34, 0x03,                                     // length
                0x49, 0x80, 0x80, 0x41, 0x24, 0x30, 0xC8, 0x00, // data
                0x05, 0x7B, 0x28, 0x0B, 0xE4, 0xDC, 0xDE, 0xD2,
                0x40, 0xD4, 0x40, 0xDC, 0x40, 0xDE, 0x40, 0x40,
                0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40,
                0x40, 0x1A, 0x14, 0x7A, 0x60, 0x76, 0x4C, 0x40,
                0xF6, 0xE4, 0xDC, 0x40, 0xF6, 0xDC, 0x40, 0xF6,
                0xE4, 0xDC, 0x40, 0xF6, 0xE4, 0xDC, 0x40, 0xF8,
                0x1A, 0x14, 0x66, 0x54, 0x64, 0x5E, 0x40, 0xF6,
                0xE4, 0xDC, 0x40, 0xF6, 0xE4, 0xDC, 0x40, 0xF6,
                0xDA, 0x40, 0xF6, 0xDE, 0x40, 0xF8, 0x1A, 0x14,
                0x58, 0x6E, 0x74, 0x66, 0x66, 0x40, 0xF6, 0xE4,
                0xDC, 0x40, 0xF6, 0xE4, 0xDC, 0x40, 0xF6, 0xE4,
                0xDC, 0x40, 0xF6, 0xD8, 0x40, 0xF6, 0xDC, 0x40,
                0xF8, 0x1A, 0x14, 0x72, 0x7C, 0x78, 0x74, 0x40,
                0xF6, 0xE4, 0xDC, 0x40, 0xF6, 0xE4, 0xDC, 0x40,
                0xF6, 0xE4, 0xDC, 0x40, 0xF6, 0xDA, 0x40, 0xF6,
                0xE4, 0xDC, 0x40, 0xF6, 0xD6, 0x40, 0x72, 0x60,
                0x5A, 0x50, 0x7C, 0x5A, 0x76, 0x40, 0x58, 0x6C,
                0x62, 0x70, 0x66, 0x74, 0x40, 0xF8, 0x1A, 0x14,
                0x58, 0x5E, 0x6E, 0x74, 0x5A, 0x74, 0xE4, 0x58,
                0x54, 0x5A, 0x72, 0x7C, 0x78, 0x74, 0x40, 0xF6,
                0xE4, 0xDC, 0x40, 0xDC, 0xDE, 0xE2, 0xDA, 0xD2,
                0xDA, 0xD4, 0xDE, 0xD8, 0xD8, 0xD6, 0xD8, 0xCC,
                0xCE, 0xD8, 0xDC, 0xDA, 0xD8, 0x40, 0xCE, 0xE2,
                0xD6, 0xD6, 0xD2, 0xDC, 0xCE, 0xD6, 0xDE, 0xD6,
                0xCC, 0xD0, 0xDE, 0xCC, 0xD4, 0xCC, 0xD0, 0x40,
                0xDE, 0x40, 0xDE, 0xE2, 0xCE, 0xD6, 0xD8, 0xDE,
                0xD4, 0xD4, 0xD2, 0xD4, 0xCE, 0xD4, 0xDC, 0xDE,
                0xDC, 0xCC, 0xCC, 0xCC, 0xDA, 0x40, 0xDC, 0x40,
                0xDE, 0x40, 0xDE, 0x40, 0xDE, 0x40, 0xDE, 0x40,
                0xDC, 0x40, 0xDE, 0x40, 0xAC, 0x40, 0xAC, 0x40,
                0xAC, 0x40, 0xAC, 0x40, 0xF8, 0x1A, 0x15, 0x60,
                0x77, 0xFC, 0xD6, 0xB3, 0x34, 0x31, 0x22, 0x01,
                0x8D, 0xFE, 0xB4, 0x78, 0xE5, 0xC8, 0x40, 0x81,
                0x20, 0x94, 0x1C, 0x0C, 0xFF, 0xFF, 0xFF, 0xFF,
                0xCF, 0xFF, 0xFF, 0xFF, 0xF4, 0x0C, 0x0E, 0xFF,
                0x9A, 0xD6, 0x66, 0x86, 0x24, 0x40, 0x32, 0x3F,
                0xD6, 0x8F, 0x1C, 0xB9, 0x08, 0x10, 0x02, 0x84,
                0xA4, 0x0D, 0xC4, 0xFF, 0xAE, 0xAB, 0xF0, 0x33,
                0xFE, 0x6B, 0x59, 0x9A, 0x18, 0x91, 0x00, 0x47,
                0xF6, 0x2D, 0x7D, 0x9A, 0x69, 0x1E, 0x40, 0x80,
                0xEF, 0xF9, 0xAD, 0x66, 0x68, 0x62, 0x44, 0x03,
                0x23, 0xFD, 0x68, 0xF1, 0xCB, 0x90, 0x81, 0x00,
                0x28, 0x4A, 0x40, 0xDC, 0x4F, 0xFA, 0xEA, 0x3F,
                0x01, 0x9F, 0xFF, 0xFF, 0xFF, 0xF9, 0xFF, 0xFF,
                0xFF, 0xFE, 0x81, 0x81, 0x9F, 0xF3, 0x5A, 0xCC,
                0xD0, 0xC4, 0x88, 0x06, 0x37, 0xFA, 0xD1, 0xE3,
                0x97, 0x21, 0x02, 0x00, 0x60, 0x94, 0x81, 0xB8,
                0x9F, 0xF5, 0xD5, 0x7E, 0x58, 0x81, 0xAF, 0xEA,
                0x05, 0x9B, 0x13, 0x20, 0x18, 0xDF, 0xEB, 0x47,
                0x8E, 0x5C, 0x84, 0x08, 0x10, 0x19, 0xFF, 0x35,
                0xAC, 0xCD, 0x0C, 0x48, 0x80, 0x63, 0x7F, 0xAD,
                0x1E, 0x39, 0x72, 0x10, 0x20, 0x06, 0x09, 0x48,
                0x1B, 0x89, 0xFF, 0x5D, 0x47, 0xE0, 0x33, 0xFF,
                0xFF, 0xFF, 0xFF, 0x3F, 0xFF, 0xFF, 0xFF, 0xD0,
                0x30, 0x3B, 0xFE, 0x6B, 0x59, 0x9A, 0x18, 0x91,
                0x00, 0xC4, 0xFF, 0x5A, 0x3C, 0x72, 0xE4, 0x20,
                0x40, 0x0C, 0x12, 0x90, 0x37, 0x13, 0xFE, 0xBA,
                0xAF, 0xC0, 0xCF, 0xF9, 0xAD, 0x66, 0x68, 0x62,
                0x44, 0x01, 0xA4, 0x10, 0x7C, 0xE8, 0x5E, 0x50,
                0x89, 0x02, 0x03, 0xBF, 0xE6, 0xB5, 0x99, 0xA1,
                0x89, 0x10, 0x0C, 0x4F, 0xF5, 0xA3, 0xC7, 0x2E,
                0x42, 0x04, 0x00, 0xC1, 0x29, 0x03, 0x71, 0x3F,
                0xEB, 0xA8, 0xFC, 0x06, 0x7F, 0xFF, 0xFF, 0xFF,
                0xE7, 0xFF, 0xFF, 0xFF, 0xFA, 0x06, 0x08, 0x7F,
                0xCD, 0x6B, 0x33, 0x43, 0x12, 0x20, 0x18, 0xDF,
                0xEB, 0x47, 0x8E, 0x5C, 0x84, 0x08, 0x01, 0x82,
                0x52, 0x06, 0xE2, 0x7F, 0xD7, 0x55, 0xF8, 0xD7,
                0xF5, 0xAD, 0xB1, 0x83, 0xAC, 0x44, 0x80, 0x61,
                0xFF, 0xAD, 0x1E, 0x39, 0x72, 0x10, 0x20, 0x40,
                0x87, 0xFC, 0xD6, 0xB3, 0x34, 0x31, 0x22, 0x01,
                0x8D, 0xFE, 0xB4, 0x78, 0xE5, 0xC8, 0x40, 0x80,
                0x18, 0x25, 0x20, 0x6E, 0x27, 0xFD, 0x75, 0x1F,
                0x80, 0x8F, 0xFF, 0xFF, 0xFF, 0xFC, 0xFF, 0xFF,
                0xFF, 0xFF, 0x40, 0xCA, 0xA3, 0x08, 0xAD, 0x62,
                0xE5, 0x52, 0x34, 0x03, 0x23, 0xFD, 0x68, 0xF1,
                0xCB, 0x90, 0x81, 0x00, 0x5B, 0xE6, 0x0C, 0x01,
                0x80, 0x13, 0xE3, 0xBF, 0x37, 0x25, 0xE4, 0xB5,
                0xB2, 0xBB, 0x48, 0xD0, 0x0F, 0x62, 0xD3, 0x7F,
                0xFC, 0x5E, 0xC2, 0x14, 0x01, 0x6F, 0x98, 0x30,
                0x06, 0x00, 0x4F, 0x8E, 0xFC, 0xDC, 0x97, 0x92,
                0xD6, 0xCA, 0xED, 0x23, 0x40, 0x0B, 0x28, 0xFF,
                0x7C, 0x8F, 0x2E, 0x07, 0xD0, 0x05, 0xBE, 0x60,
                0xC0, 0x18, 0x01, 0x3E, 0x3B, 0xF0, 0x11, 0xFF,
                0xFF, 0xFF, 0xFF, 0x9F, 0xFF, 0xFF, 0xFF, 0xE8,
                0x18, 0xD7, 0xF5, 0xAD, 0xB1, 0x83, 0xAC, 0x44,
                0x80, 0x64, 0xFF, 0xAD, 0x1E, 0x39, 0x72, 0x10,
                0x20, 0x45, 0xEF, 0xE5, 0xC2, 0xBC, 0xA5, 0x71,
                0x1A, 0x00, 0x8B, 0x93, 0x7D, 0xCC, 0x84, 0xB4,
                0x44, 0x81, 0x17, 0x9F, 0x97, 0x0A, 0xF2, 0x95,
                0xC4, 0x68, 0x04, 0x73, 0x67, 0x71, 0x1A, 0x1E,
                0xE8, 0xF2, 0x04, 0x02, 0x3F, 0xFF, 0xFF, 0xFF,
                0xF3, 0xFF, 0xFF, 0xFF, 0xFD, 0x03, 0x2A, 0x8C,
                0x22, 0xB5, 0x8B, 0x95, 0x48, 0xD0, 0x0C, 0x8F,
                0xF5, 0xA3, 0xC7, 0x2E, 0x42, 0x04, 0x01, 0x6F,
                0x98, 0x30, 0x06, 0x00, 0x4F, 0x8C, 0xFC, 0xDC,
                0x97, 0x92, 0xD6, 0xCA, 0xED, 0x23, 0x40, 0x3D,
                0x8B, 0x4D, 0xFF, 0xF1, 0x7B, 0x08, 0x50, 0x05,
                0xBE, 0x60, 0xC0, 0x18, 0x01, 0x3E, 0x33, 0xF3,
                0x72, 0x5E, 0x4B, 0x5B, 0x2B, 0xB4, 0x8D, 0x00,
                0x2C, 0xA3, 0xFD, 0xF2, 0x3C, 0xB8, 0x1F, 0x40,
                0x16, 0xF9, 0x83, 0x00, 0x60, 0x04, 0xF8, 0xCF,
                0xD4, 0xC1, 0x44, 0x3E,
                0x5A, 0xC5                                      // crc
            );
            Assert.Equal(new DwgHandle(0x0104), solid.Handle);
            Assert.Equal(242, solid.RawData.Count);
            Assert.Equal(561, solid.TrailingData.Count);
            Assert.Equal(5, solid.FinalBitCount);
            Assert.Equal(0x15, solid.FinalByte);
        }

        [Fact]
        public void ReadRawMText()
        {
            var mtext = (DwgMText)ParseRaw(
                0x4D, 0x00,                                     // length
                0x4B, 0x00, 0x73, 0xA0, 0xC8, 0x10, 0x00, 0x01, // data
                0x33, 0x0F, 0xAE, 0x2B, 0x5E, 0xAE, 0xE0, 0x84,
                0x48, 0x04, 0x88, 0x93, 0xFD, 0xFD, 0x9A, 0x00,
                0xFA, 0x05, 0x4B, 0x50, 0x15, 0xAF, 0x46, 0xE0,
                0x7A, 0x15, 0x8E, 0x7E, 0x82, 0xA0, 0x20, 0x6E,
                0xBD, 0x1B, 0x81, 0xE8, 0x56, 0x39, 0xF9, 0x9B,
                0x99, 0x99, 0x99, 0x99, 0x99, 0xE0, 0x7E, 0x85,
                0xAE, 0x20, 0x98, 0x9D, 0xA9, 0x18, 0x17, 0x1B,
                0x1B, 0x19, 0x1B, 0x60, 0x82, 0x18, 0x28, 0x87,
                0xA8, 0x89, 0xA8, 0x88, 0x00,
                0x6F, 0xF0                                      // crc
            );
            Assert.Equal(new DwgHandle(0xCE), mtext.Handle);
            Assert.Equal(new DwgPoint(9.008727710047191, 7.813185684328929, 0.0), mtext.InsertionPoint);
            Assert.Equal(DwgVector.ZAxis, mtext.Extrusion);
            Assert.Equal(DwgVector.XAxis, mtext.XAxisDirection);
            Assert.Equal(0.0, mtext.RectangleWidth);
            Assert.Equal(0.18, mtext.TextHeight);
            Assert.Equal(DwgAttachmentPoint.MiddleCenter, mtext.Attachment);
            Assert.Equal(DwgDrawingDirection.LeftToRight, mtext.DrawingDirection);
            Assert.Equal(0.18000000000000008, mtext.ExtentsHeight);
            Assert.Equal(1.05, mtext.ExtentsWidth);
            Assert.Equal("\\A1;R0.6626", mtext.Text);
        }

        [Fact]
        public void ReadRawLeader()
        {
            var leader = (DwgLeader)ParseRaw(
                0x80, 0x00,                                     // length
                0x4B, 0x40, 0x80, 0x42, 0x65, 0x20, 0x18, 0x00, // data
                0x05, 0x5B, 0x29, 0x03, 0x25, 0xAD, 0x59, 0x2D,
                0x08, 0x7D, 0xC9, 0x50, 0x04, 0x41, 0xFF, 0xAB,
                0xAF, 0xA2, 0x81, 0x04, 0x08, 0x2E, 0xE6, 0x9D,
                0x29, 0x5D, 0x0C, 0x21, 0x40, 0x1C, 0x3C, 0xC0,
                0x0F, 0xB5, 0xED, 0x05, 0xD0, 0x20, 0x50, 0x04,
                0x84, 0x77, 0x1E, 0x34, 0x65, 0x00, 0x78, 0x56,
                0x18, 0x21, 0xBF, 0xAB, 0x15, 0x40, 0x89, 0x6B,
                0x56, 0x4B, 0x42, 0x1F, 0x72, 0x54, 0x01, 0x10,
                0x7F, 0xEA, 0xEB, 0xE8, 0xA0, 0x41, 0x02, 0xA5,
                0xAA, 0xAA, 0x02, 0xB5, 0xE8, 0xDC, 0x0F, 0x42,
                0xAD, 0xCF, 0xC0, 0xAD, 0x7A, 0x37, 0x03, 0xD0,
                0xAC, 0x73, 0xF3, 0x0B, 0xD4, 0xA1, 0x72, 0x3F,
                0x0B, 0xB4, 0xFF, 0x80, 0xAD, 0x7A, 0x37, 0x03,
                0xD0, 0xAC, 0x73, 0xF2, 0xC3, 0x05, 0x10, 0xFC,
                0x10, 0x26, 0x05, 0x20, 0x10, 0xA5, 0x11, 0xD6,
                0x6E, 0xAB                                      // crc
            );
            Assert.Equal(new DwgHandle(0x0109), leader.Handle);
            Assert.False(leader._unknownBit1);
            Assert.Equal(DwgAnnotationType.MText, leader.AnnotationType);
            Assert.Equal(0, leader.PathType);
            Assert.Equal(3, leader.Points.Count);
            Assert.Equal(new DwgPoint(10.982679021161669, 4.0400189604914125, 0.0), leader.Points[0]);
            Assert.Equal(new DwgPoint(8.524148273968503, 5.926602333839455, 0.0), leader.Points[1]);
            Assert.Equal(new DwgPoint(6.388454871310632, 5.417721287832698, 0.0), leader.Points[2]);
            Assert.Equal(new DwgVector(10.982679021161669, 4.0400189604914125, 0.0), leader.EndPointProjection);
            Assert.Equal(DwgVector.ZAxis, leader.Extrusion);
            Assert.Equal(DwgVector.XAxis, leader.XAxisDirection);
            Assert.Equal(DwgPoint.Origin, leader.OffsetBlockInsertionPoint);
            Assert.Equal(DwgPoint.Origin, leader._unknownPoint);
            Assert.Equal(0.09, leader.DimensionLineGap);
            Assert.Equal(0.18, leader.BoxHeight);
            Assert.Equal(0.92999999999999994, leader.BoxWidth);
            Assert.True(leader.IsHookLineOnXAxis);
            Assert.True(leader.IsArrowheadOnIndicator);
            Assert.Equal(0, leader.ArrowheadType);
            Assert.Equal(0.18, leader.DimensioningArrowSize);
            Assert.False(leader._unknownBit2);
            Assert.False(leader._unknownBit3);
            Assert.Equal(0, leader._unknownShort);
            Assert.Equal(DwgColor.ByLayer, leader.ByBlockColor);
            Assert.False(leader._unknownBit4);
            Assert.False(leader._unknownBit5);
            Assert.Equal(new DwgHandle(0x010A), leader.ResolveHandleReference(leader._associatedAnnotationHandleReference));
            Assert.Equal(new DwgHandle(0x1D), leader.ResolveHandleReference(leader._dimStyleHandleReference));
        }

        [Fact]
        public void ReadRawTolerance()
        {
            var tol = (DwgTolerance)ParseRaw(
                0x65, 0x00,                                     // length
                0x4B, 0x80, 0x80, 0x43, 0x27, 0x18, 0x10, 0x00, // data
                0x05, 0x5B, 0x40, 0x56, 0xBD, 0x1B, 0x81, 0xE8,
                0x56, 0x39, 0xF8, 0x15, 0xAF, 0x46, 0xE0, 0x7A,
                0x15, 0x6E, 0x7E, 0x51, 0x19, 0xA0, 0x47, 0x00,
                0xC7, 0x13, 0xA0, 0x18, 0x09, 0x38, 0x21, 0x8A,
                0x5E, 0xA1, 0x48, 0x13, 0x54, 0xA5, 0xCF, 0x6B,
                0x88, 0xCC, 0xEC, 0x8E, 0x87, 0x6D, 0x4F, 0xA4,
                0xA4, 0xAE, 0xCF, 0x6B, 0x88, 0xCC, 0xEC, 0x8E,
                0x87, 0x6D, 0xCF, 0xAC, 0x2E, 0x6C, 0x8C, 0xCF,
                0x6B, 0x88, 0xCC, 0xEC, 0x8E, 0x87, 0x6D, 0xAF,
                0xA4, 0xA4, 0xAE, 0xC4, 0xA4, 0xAE, 0xC4, 0xA4,
                0xAE, 0xC4, 0xA4, 0xAE, 0xC6, 0x0A, 0x21, 0xF8,
                0x20, 0x4C, 0x0A, 0x23, 0xB4,
                0x45, 0x2F                                      // crc
            );
            Assert.Equal(new DwgHandle(0x010C), tol.Handle);
            Assert.Equal(0, tol._unknownShort);
            Assert.Equal(0.18, tol.Height);
            Assert.Equal(0.09, tol.DimensionLineGap);
            Assert.Equal(new DwgPoint(11.77735561879837, 3.3697854038702815, 0.0), tol.InsertionPoint);
            Assert.Equal(DwgVector.XAxis, tol.XAxisDirection);
            Assert.Equal(DwgVector.ZAxis, tol.Extrusion);
            Assert.Equal("{\\Fgdt;j}%%v{\\Fgdt;n}asdf{\\Fgdt;m}%%v%%v%%v%%v", tol.Text);
        }

        [Fact]
        public void ReadRawMLine()
        {
            var ml = (DwgMLine)ParseRaw(
                0xE4, 0x00,                                     // length
                0x4B, 0xC0, 0x80, 0x43, 0x66, 0xC8, 0x30, 0x00, // data
                0x05, 0x5B, 0x20, 0x04, 0x61, 0xAD, 0x1E, 0xF2,
                0x13, 0xA8, 0x8A, 0x01, 0x81, 0x93, 0x3C, 0x67,
                0xD4, 0x2B, 0xC0, 0x7F, 0x52, 0x80, 0x81, 0x20,
                0x64, 0x61, 0xAD, 0x1E, 0xF2, 0x13, 0xA8, 0x8A,
                0x01, 0x81, 0x93, 0x3C, 0x67, 0xD4, 0x2B, 0xC0,
                0x7F, 0x13, 0xE9, 0xCF, 0x70, 0xDE, 0x47, 0x3C,
                0xC7, 0xE4, 0xF8, 0x6A, 0x7B, 0x9C, 0x00, 0x2F,
                0x39, 0xFC, 0x4E, 0x86, 0xA7, 0xB9, 0xC0, 0x02,
                0xF3, 0xDF, 0x93, 0xE9, 0xCF, 0x70, 0xDE, 0x47,
                0x3C, 0xC7, 0xF2, 0x05, 0x52, 0x04, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x78, 0x5F, 0xD0, 0x38,
                0xDD, 0x69, 0xB0, 0x78, 0xDE, 0x38, 0x80, 0x44,
                0x0A, 0x1C, 0x33, 0xBD, 0xE1, 0x05, 0x20, 0x40,
                0x62, 0xC6, 0x53, 0x15, 0xC9, 0x57, 0x71, 0xF9,
                0xFB, 0x6C, 0xF0, 0x9B, 0x58, 0xB3, 0xAB, 0x7F,
                0x05, 0x47, 0x3C, 0xF4, 0x7B, 0x0B, 0x79, 0xB7,
                0xE0, 0x8F, 0x92, 0x1D, 0xDC, 0xD1, 0x2F, 0x79,
                0xFC, 0x81, 0x54, 0x81, 0x18, 0xD6, 0xBA, 0x82,
                0xC0, 0x20, 0xDE, 0x77, 0xF4, 0x27, 0x50, 0xA1,
                0x65, 0x46, 0x02, 0x92, 0xA0, 0x13, 0x16, 0x15,
                0x43, 0xDA, 0x24, 0x00, 0x28, 0x10, 0x1C, 0xB1,
                0x94, 0xC5, 0x72, 0x55, 0xDC, 0x7E, 0x7F, 0x5B,
                0x3C, 0x26, 0xD6, 0x2C, 0xEA, 0xDF, 0xC7, 0x65,
                0xB3, 0xC2, 0x6D, 0x62, 0xCE, 0xA9, 0xF8, 0x20,
                0xB1, 0x94, 0xC5, 0x72, 0x55, 0xDC, 0x7F, 0x20,
                0x55, 0x20, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x07, 0x85, 0xFD, 0x18, 0x28, 0x87, 0xC0, 0x50,
                0x87, 0x28, 0x8E, 0x4C,
                0x91, 0x88                                      // crc
            );
            Assert.Equal(new DwgHandle(0x010D), ml.Handle);
            Assert.Equal(1.0, ml.Scale);
            Assert.Equal(DwgJustification.Top, ml.Justification);
            Assert.Equal(new DwgPoint(4.36451290007734, 0.5026751526077575, 0.0), ml.BasePoint);
            Assert.Equal(DwgVector.ZAxis, ml.Extrusion);
            Assert.False(ml.IsClosed);
            Assert.Equal(3, ml.Vertices.Count);
            Assert.Equal(new DwgPoint(4.36451290007734, 0.5026751526077575, 0.0), ml.Vertices[0].Location);
            Assert.Equal(new DwgVector(0.6945736528964125, 0.71942160149813, 0.0), ml.Vertices[0].VertexDirection);
            Assert.Equal(new DwgVector(-0.7194216014981297, 0.6945736528964125, 0.0), ml.Vertices[0].MiterDirection);
            Assert.Equal(2, ml.Vertices[0].Styles.Count);
            Assert.Equal(new[] { 0.0, 0.0 }, ml.Vertices[0].Styles[0].SegmentParameters);
            Assert.Equal(Array.Empty<double>(), ml.Vertices[0].Styles[0].AreaFillParameters);
            Assert.Equal(new[] { -1.0, 0.0 }, ml.Vertices[0].Styles[1].SegmentParameters);
            Assert.Equal(Array.Empty<double>(), ml.Vertices[0].Styles[1].AreaFillParameters);
            Assert.Equal(new DwgPoint(7.10863054848917, 3.344961935423786, 0.0), ml.Vertices[1].Location);
            Assert.Equal(new DwgVector(0.9427153520107381, -0.3335982090528516, 0.0), ml.Vertices[1].VertexDirection);
            Assert.Equal(new DwgVector(-0.22936542513930053, 0.9733403832938753, 0.0), ml.Vertices[1].MiterDirection);
            Assert.Equal(2, ml.Vertices[0].Styles.Count);
            Assert.Equal(new[] { 0.0, 0.0 }, ml.Vertices[1].Styles[0].SegmentParameters);
            Assert.Equal(Array.Empty<double>(), ml.Vertices[1].Styles[0].AreaFillParameters);
            Assert.Equal(new[] { -1.1889658824760416, 0.0 }, ml.Vertices[1].Styles[1].SegmentParameters);
            Assert.Equal(Array.Empty<double>(), ml.Vertices[1].Styles[1].AreaFillParameters);
            Assert.Equal(new DwgPoint(10.510839768060261, 2.1410238643494033, 0.0), ml.Vertices[2].Location);
            Assert.Equal(new DwgVector(0.9427153520107383, -0.33359820905285165, 0.0), ml.Vertices[2].VertexDirection);
            Assert.Equal(new DwgVector(0.33359820905285065, 0.9427153520107385, 0.0), ml.Vertices[2].MiterDirection);
            Assert.Equal(2, ml.Vertices[0].Styles.Count);
            Assert.Equal(new[] { 0.0, 0.0 }, ml.Vertices[2].Styles[0].SegmentParameters);
            Assert.Equal(Array.Empty<double>(), ml.Vertices[2].Styles[0].AreaFillParameters);
            Assert.Equal(new[] { -1.0, 0.0 }, ml.Vertices[2].Styles[1].SegmentParameters);
            Assert.Equal(Array.Empty<double>(), ml.Vertices[2].Styles[1].AreaFillParameters);
            Assert.Equal(new DwgHandle(0x1C), ml.ResolveHandleReference(ml._mlineStyleHandleReference));
        }

        [Fact]
        public void ReadRawRay()
        {
            var ray = (DwgRay)ParseRaw(
                0x2F, 0x00,                                     // length
                0x4A, 0x00, 0x80, 0x41, 0xA2, 0xE8, 0x08, 0x00, // data
                0x05, 0x7B, 0x12, 0x4C, 0x98, 0x47, 0xCA, 0xEF,
                0xC4, 0xA8, 0x00, 0x84, 0x0B, 0xFC, 0x98, 0x72,
                0x3F, 0xF9, 0xFC, 0x0F, 0x91, 0xCC, 0xD7, 0xE5,
                0xCD, 0x71, 0x5F, 0x99, 0x7D, 0x7E, 0x4D, 0x05,
                0xC1, 0x3D, 0x47, 0xF1, 0x82, 0x88, 0x78,
                0xAD, 0xCF                                      // crc
            );
            Assert.Equal(new DwgHandle(0x0106), ray.Handle);
            Assert.Equal(new DwgPoint(10.746759377698513, 1.9548478852885331, 0.0), ray.Point);
            Assert.Equal(new DwgVector(-0.5813959531452361, 0.8136207628043562, 0.0), ray.Vector);
        }

        [Fact]
        public void ReadRawXLine()
        {
            var xl = (DwgXLine)ParseRaw(
                0x2F, 0x00,                                     // length
                0x4A, 0x40, 0x80, 0x41, 0x62, 0xE8, 0x08, 0x00, // data
                0x05, 0x7B, 0x09, 0x95, 0x83, 0x71, 0xC8, 0xB2,
                0x03, 0xE8, 0x03, 0xC1, 0x22, 0x6C, 0x91, 0x8A,
                0x28, 0x4A, 0x04, 0x28, 0xB1, 0x0A, 0x5D, 0xF6,
                0x48, 0x76, 0x1F, 0x8D, 0xE0, 0xE8, 0x59, 0xD8,
                0x7A, 0xFB, 0x87, 0xF1, 0x82, 0x88, 0x78,
                0xFE, 0x9C                                      // crc
            );
            Assert.Equal(new DwgHandle(0x0105), xl.Handle);
            Assert.Equal(new DwgPoint(7.890890331687, 3.158785956362916, 0.0), xl.Point);
            Assert.Equal(new DwgVector(0.8926910070322923, 0.4506692423093374, 0.0), xl.Vector);
        }

        [Fact]
        public void ReadRawDictionary()
        {
            var dict = (DwgDictionary)ParseRaw(
                0x2C, 0x00,                                     // length
                0x4A, 0x80, 0x43, 0x22, 0xC0, 0x10, 0x00, 0x09, // data
                0x02, 0x00, 0x42, 0x90, 0x50, 0xD0, 0x51, 0x17,
                0xD1, 0xD4, 0x93, 0xD5, 0x54, 0x10, 0xF4, 0x14,
                0x34, 0x14, 0x45, 0xF4, 0xD4, 0xC4, 0x94, 0xE4,
                0x55, 0x35, 0x45, 0x94, 0xC4, 0x54, 0x03, 0x02,
                0x10, 0xD2, 0x10, 0xEC,
                0xD2, 0x36                                      // crc
            );
            Assert.Equal(new DwgHandle(0x0C), dict.Handle);
            Assert.Equal(2, dict._entityCount);
            Assert.Equal("ACAD_GROUP", dict._names[0]);
            Assert.Equal("ACAD_MLINESTYLE", dict._names[1]);
            Assert.Equal(new DwgHandle(0x0D), dict.ResolveHandleReference(dict._entityHandleReferences[0]));
            Assert.Equal(new DwgHandle(0x0E), dict.ResolveHandleReference(dict._entityHandleReferences[1]));
        }

        [Fact]
        public void ReadRawBlockControl()
        {
            var bc = (DwgBlockControlObject)ParseRaw(
                0x20, 0x00,                                     // length
                0x4C, 0x00, 0x40, 0x64, 0x80, 0x00, 0x00, 0x09, // data
                0x08, 0x40, 0x30, 0x21, 0x93, 0x21, 0x9F, 0x21,
                0xAD, 0x21, 0xBB, 0x21, 0xCA, 0x21, 0xD6, 0x21,
                0xF4, 0x22, 0x01, 0x13, 0x31, 0x19, 0x31, 0x16,
                0xC1, 0x3A                                      // crc
            );
            Assert.Equal(new DwgHandle(0x01), bc.Handle);
            Assert.Equal(8, bc._entityCount);
            Assert.Equal(new DwgHandle(0x93), bc.ResolveHandleReference(bc._entityHandleReferences[0]));
            Assert.Equal(new DwgHandle(0x9F), bc.ResolveHandleReference(bc._entityHandleReferences[1]));
            Assert.Equal(new DwgHandle(0xAD), bc.ResolveHandleReference(bc._entityHandleReferences[2]));
            Assert.Equal(new DwgHandle(0xBB), bc.ResolveHandleReference(bc._entityHandleReferences[3]));
            Assert.Equal(new DwgHandle(0xCA), bc.ResolveHandleReference(bc._entityHandleReferences[4]));
            Assert.Equal(new DwgHandle(0xD6), bc.ResolveHandleReference(bc._entityHandleReferences[5]));
            Assert.Equal(new DwgHandle(0xF4), bc.ResolveHandleReference(bc._entityHandleReferences[6]));
            Assert.Equal(new DwgHandle(0x0113), bc.ResolveHandleReference(bc._entityHandleReferences[7]));
            Assert.Equal(new DwgHandle(0x19), bc.ResolveHandleReference(bc._modelSpaceBlockHeaderHandleReference));
            Assert.Equal(new DwgHandle(0x16), bc.ResolveHandleReference(bc._paperSpaceBlockHeaderHandleReference));
        }

        [Fact]
        public void ReadRawBlockHeader()
        {
            var b = (DwgBlockHeader)ParseRaw(
                0x19, 0x00,                                     // length
                0x4C, 0x40, 0x72, 0xA6, 0x80, 0x00, 0x00, 0x09, // data
                0x02, 0x2A, 0x44, 0xC8, 0xAA, 0x41, 0x01, 0x30,
                0x50, 0x31, 0xCB, 0x41, 0xCC, 0x41, 0xD3, 0x31,
                0xD4,
                0xE5, 0xAA                                      // crc
            );
            Assert.Equal(new DwgHandle(0xCA), b.Handle);
            Assert.Equal("*D", b.Name);
            Assert.True(b._64flag);
            Assert.Equal(0, b._xrefIndex);
            Assert.False(b._isDependentOnXRef);
            Assert.True(b.IsAnonymous);
            Assert.False(b.HasAttributes);
            Assert.False(b.IsXRef);
            Assert.False(b.IsOverlaidXref);
            Assert.Equal(DwgPoint.Origin, b.BasePoint);
            Assert.Equal("", b.PathName);
            Assert.Equal(new DwgHandle(0x01), b.ResolveHandleReference(b.BlockControlHandleReference));
            Assert.Equal(new DwgHandle(0xCB), b.ResolveHandleReference(b.BlockEntityHandleReference));
            Assert.Equal(new DwgHandle(0xD4), b.ResolveHandleReference(b.EndBlockEntityHandleReference));
        }

        [Fact]
        public void ReadRawLayerControl()
        {
            var lc = (DwgLayerControlObject)ParseRaw(
                0x0F, 0x00,                                     // length
                0x4C, 0x80, 0x40, 0xA4, 0x80, 0x00, 0x00, 0x09, // data
                0x02, 0x40, 0x30, 0x21, 0x0F, 0x21, 0x99,
                0xC3, 0x1D                                      // crc
            );
            Assert.Equal(new DwgHandle(0x02), lc.Handle);
            Assert.Equal(2, lc._entityCount);
            Assert.Equal(new DwgHandle(0x0F), lc.ResolveHandleReference(lc._entityHandleReferences[0]));
            Assert.Equal(new DwgHandle(0x99), lc.ResolveHandleReference(lc._entityHandleReferences[1]));
        }

        [Fact]
        public void ReadRawLayer()
        {
            var l = (DwgLayer)ParseRaw(
                0x1B, 0x00,                                     // length
                0x4C, 0xC0, 0x66, 0x6A, 0x20, 0x00, 0x00, 0x09, // data
                0x09, 0x44, 0x45, 0x46, 0x50, 0x4F, 0x49, 0x4E,
                0x54, 0x53, 0xC0, 0x41, 0xD0, 0x40, 0x8C, 0x14,
                0x14, 0x45, 0x48,
                0x34, 0x8F                                      // crc
            );
            Assert.Equal(new DwgHandle(0x99), l.Handle);
            Assert.Equal("DEFPOINTS", l.Name);
            Assert.True(l._64flag);
            Assert.Equal(0, l._xrefIndex);
            Assert.False(l._isDependentOnXRef);
            Assert.False(l.IsFrozen);
            Assert.False(l.IsOn);
            Assert.False(l.IsFrozenInNew);
            Assert.False(l.IsLocked);
            Assert.Equal(0x07, l.Color.RawValue);
            Assert.Equal(new DwgHandle(0x15), l.ResolveHandleReference(l._lineTypeHandleReference));
        }

        [Fact]
        public void ReadRawStyleControl()
        {
            var sc = (DwgStyleControlObject)ParseRaw(
                0x0F, 0x00,                                     // length
                0x4D, 0x00, 0x40, 0xE4, 0x80, 0x00, 0x00, 0x09, // data
                0x02, 0x40, 0x30, 0x21, 0x10, 0x21, 0xF3,
                0x33, 0x8B                                      // crc
            );
            Assert.Equal(new DwgHandle(0x03), sc.Handle);
            Assert.Equal(2, sc._entityCount);
            Assert.Equal(new DwgHandle(0x10), sc.ResolveHandleReference(sc._entityHandleReferences[0]));
            Assert.Equal(new DwgHandle(0xF3), sc.ResolveHandleReference(sc._entityHandleReferences[1]));
        }

        [Fact]
        public void ReadRawStyle()
        {
            var s = (DwgStyle)ParseRaw(
                0x25, 0x00,                                     // length
                0x4D, 0x40, 0x44, 0x20, 0x20, 0x10, 0x00, 0x09, // data
                0x08, 0x53, 0x54, 0x41, 0x4E, 0x44, 0x41, 0x52,
                0x44, 0xC2, 0x60, 0x02, 0x6A, 0x66, 0x66, 0x66,
                0x66, 0x67, 0x24, 0xFD, 0x03, 0x74, 0x78, 0x74,
                0x90, 0x40, 0xCC, 0x14, 0x28,
                0xEC, 0x6E                                      // crc
            );
            Assert.Equal(new DwgHandle(0x10), s.Handle);
            Assert.Equal("STANDARD", s.Name);
            Assert.True(s._64flag);
            Assert.Equal(0, s._xrefIndex);
            Assert.False(s._isDependentOnXRef);
            Assert.False(s.IsVertical);
            Assert.False(s.IsShapeFile);
            Assert.Equal(0.0, s.FixedHeight);
            Assert.Equal(1.0, s.WidthFactor);
            Assert.Equal(0.0, s.ObliqueAngle);
            Assert.Equal(0x00, s._generationFlags);
            Assert.Equal(0.2, s.LastHeight);
            Assert.Equal("txt", s.FontName);
            Assert.Equal("", s.BigFontName);
            Assert.Equal(new DwgHandle(0x03), s.ResolveHandleReference(s.StyleControlHandleReference));
        }

        [Fact]
        public void ReadRawLineTypeControl()
        {
            var lc = (DwgLineTypeControlObject)ParseRaw(
                0x11, 0x00,                                     // length
                0x4E, 0x00, 0x41, 0x64, 0x80, 0x00, 0x00, 0x09, // data
                0x01, 0x40, 0x30, 0x21, 0x15, 0x31, 0x13, 0x31,
                0x14,
                0x82, 0x54                                      // crc
            );
            Assert.Equal(new DwgHandle(0x05), lc.Handle);
            Assert.Equal(1, lc._entityCount);
            Assert.Equal(new DwgHandle(0x15), lc.ResolveHandleReference(lc._entityHandleReferences[0]));
            Assert.Equal(new DwgHandle(0x13), lc.ResolveHandleReference(lc._byLayerHandleReference));
            Assert.Equal(new DwgHandle(0x14), lc.ResolveHandleReference(lc._byBlockHandleReference));
        }

        [Fact]
        public void ReadRawViewControl()
        {
            var vc = (DwgViewControlObject)ParseRaw(
                0x0D, 0x00,                                     // length
                0x4F, 0x00, 0x41, 0xA4, 0x80, 0x00, 0x00, 0x09, // data
                0x01, 0x40, 0x30, 0x21, 0x3F,
                0xE1, 0x20                                      // crc
            );
            Assert.Equal(new DwgHandle(0x06), vc.Handle);
            Assert.Equal(1, vc._entityCount);
            Assert.Equal(new DwgHandle(0x3F), vc.ResolveHandleReference(vc._entityHandleReferences[0]));
        }

        [Fact]
        public void ReadRawView()
        {
            var v = (DwgView)ParseRaw(
                0x40, 0x00,                                     // length
                0x4F, 0x40, 0x4F, 0xED, 0x90, 0x10, 0x00, 0x09, // data
                0x06, 0x4D, 0x59, 0x56, 0x49, 0x45, 0x57, 0xC2,
                0xF1, 0x38, 0x4A, 0xE7, 0xEB, 0xB4, 0xA9, 0x00,
                0x9E, 0xEA, 0x45, 0x5D, 0x73, 0x27, 0x34, 0x40,
                0x9D, 0xEA, 0x45, 0x5D, 0x73, 0x27, 0x24, 0x40,
                0xBC, 0x4E, 0x12, 0xB9, 0xFA, 0xED, 0x1A, 0x40,
                0xAA, 0x98, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x49, 0x40, 0xA1, 0x20, 0x83, 0x18, 0x28, 0x00,
                0x0C, 0x90                                      // crc
            );
            Assert.Equal(new DwgHandle(0x3F), v.Handle);
            Assert.Equal("MYVIEW", v.Name);
            Assert.True(v._64flag);
            Assert.Equal(0, v._xrefIndex);
            Assert.False(v._isDependentOnXRef);
            Assert.Equal(13.464803489193734, v.Height);
            Assert.Equal(20.154104070252849, v.Width);
            Assert.Equal(new DwgPoint(10.077052035126423, 6.732401744596867, 0.0), v.Center);
            Assert.Equal(DwgPoint.Origin, v.Target);
            Assert.Equal(DwgVector.ZAxis, v.Direction);
            Assert.Equal(0.0, v.TwistAngle);
            Assert.Equal(50.0, v.LensLength);
            Assert.Equal(0.0, v.FrontClip);
            Assert.Equal(0.0, v.BackClip);
            Assert.Equal(0x01, v._viewModeBits);
            Assert.False(v.IsInPaperSpace);
            Assert.Equal(new DwgHandle(0x06), v.ResolveHandleReference(v.ViewControlHandleReference));
        }

        [Fact]
        public void ReadRawUcsControl()
        {
            var uc = (DwgUCSControlObject)ParseRaw(
                0x0D, 0x00,                                     // length
                0x4F, 0x80, 0x41, 0xE4, 0x80, 0x00, 0x00, 0x09, // data
                0x01, 0x40, 0x30, 0x21, 0x4C,
                0xA0, 0x6F                                      // crc
            );
            Assert.Equal(new DwgHandle(0x07), uc.Handle);
            Assert.Equal(1, uc._entityCount);
            Assert.Equal(new DwgHandle(0x4C), uc.ResolveHandleReference(uc._entityHandleReferences[0]));
        }

        [Fact]
        public void ReadRawUcs()
        {
            var u = (DwgUCS)ParseRaw(
                0x45, 0x00,                                     // length
                0x4F, 0xC0, 0x53, 0x20, 0x60, 0x20, 0x00, 0x09, // data
                0x05, 0x4D, 0x59, 0x55, 0x43, 0x53, 0xCA, 0x8F,
                0xDF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE, 0x73, 0xF2,
                0x14, 0xE5, 0x08, 0xBB, 0x73, 0x23, 0x90, 0xFC,
                0xEC, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xBF, 0xBF,
                0x2B, 0xD3, 0x16, 0x3A, 0x1E, 0xAD, 0xB6, 0xEF,
                0xCF, 0x8F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE, 0x33,
                0xF2, 0x18, 0xE5, 0x08, 0xBB, 0x73, 0x23, 0x90,
                0xFD, 0x04, 0x1C, 0xC1, 0x40,
                0xBE, 0x62                                      // crc
            );
            Assert.Equal(new DwgHandle(0x4C), u.Handle);
            Assert.Equal("MYUCS", u.Name);
            Assert.True(u._64flag);
            Assert.Equal(0, u._xrefIndex);
            Assert.False(u._isDependentOnXRef);
            Assert.Equal(DwgPoint.Origin, u.Origin);
            Assert.Equal(new DwgVector(0.7499999999999997, 0.6495190528383296, -0.12499999999999972), u.XDirection);
            Assert.Equal(new DwgVector(-0.4330127018922196, 0.6249999999999991, 0.6495190528383297), u.YDirection);
            Assert.Equal(new DwgHandle(0x07), u.ResolveHandleReference(u.UCSControlHandleReference));
        }

        [Fact]
        public void ReadRawViewPortControl()
        {
            var vc = (DwgViewPortControlObject)ParseRaw(
                0x12, 0x00,                                     // length
                0x50, 0x00, 0x42, 0x24, 0x80, 0x00, 0x00, 0x09, // data
                0x04, 0x40, 0x30, 0x20, 0x21, 0x4E, 0x21, 0x4F,
                0x21, 0x50,
                0x9E, 0x1F                                      // crc
            );
            Assert.Equal(new DwgHandle(0x08), vc.Handle);
            Assert.Equal(4, vc._entityCount);
            Assert.Equal(new DwgHandle(0x00), vc.ResolveHandleReference(vc._entityHandleReferences[0]));
            Assert.Equal(new DwgHandle(0x4E), vc.ResolveHandleReference(vc._entityHandleReferences[1]));
            Assert.Equal(new DwgHandle(0x4F), vc.ResolveHandleReference(vc._entityHandleReferences[2]));
            Assert.Equal(new DwgHandle(0x50), vc.ResolveHandleReference(vc._entityHandleReferences[3]));
        }

        [Fact]
        public void ReadRawViewPort()
        {
            var v = (DwgViewPort)ParseRaw(
                0x93, 0x00,                                     // length
                0x50, 0x40, 0x53, 0xA7, 0x50, 0x40, 0x00, 0x09, // data
                0x07, 0x2A, 0x41, 0x43, 0x54, 0x49, 0x56, 0x45,
                0xC2, 0x1E, 0x94, 0x3B, 0x21, 0xCD, 0xA4, 0xCD,
                0x00, 0xA5, 0x86, 0x68, 0x4A, 0x2C, 0x0E, 0x2D,
                0x40, 0xA5, 0x86, 0x68, 0x4A, 0x2C, 0x0E, 0x1D,
                0x40, 0x87, 0xA5, 0x0E, 0xC8, 0x73, 0x69, 0x23,
                0x40, 0xAA, 0x98, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x49, 0x40, 0xA1, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0xE0, 0x3F, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0xF0, 0x3F, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0xF0, 0x3F, 0x2C, 0x98, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x01, 0xC0, 0x7E, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x01, 0xC0, 0x7E, 0x50, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x07, 0x01, 0xF8, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x07, 0x01, 0xFA, 0x08,
                0x41, 0x82, 0x80,
                0x7D, 0x31                                      // crc
            );
            Assert.Equal(new DwgHandle(0x4E), v.Handle);
            Assert.Equal("*ACTIVE", v.Name);
            Assert.True(v._64flag);
            Assert.Equal(0, v._xrefIndex);
            Assert.False(v._isDependentOnXRef);
            Assert.Equal(19.411922935081318, v.Height);
            Assert.Equal(14.527681660899654, v.AspectRatio);
            Assert.Equal(new DwgPoint(7.263840830449827, 9.705961467540659, 0.0), v.Center);
            Assert.Equal(DwgPoint.Origin, v.Target);
            Assert.Equal(DwgVector.ZAxis, v.Direction);
            Assert.Equal(0.0, v.TwistAngle);
            Assert.Equal(50.0, v.LensLength);
            Assert.Equal(0.0, v.FrontClip);
            Assert.Equal(0.0, v.BackClip);
            Assert.Equal(0x01, v._viewModeBits);
            Assert.Equal(new DwgPoint(0.5, 0.0, 0.0), v.LowerLeft);
            Assert.Equal(new DwgPoint(1.0, 1.0, 0.0), v.UpperRight);
            Assert.False(v.UCSFollow);
            Assert.Equal(100, v.CircleZoomPercent);
            Assert.True(v.FastZoom);
            Assert.Equal(0x02, v._ucsIconBits);
            Assert.False(v.IsGridOn);
            Assert.Equal(new DwgVector(0.5, 0.5, 0.0), v.GridSpacing);
            Assert.False(v.IsSnapOn);
            Assert.False(v.SnapStyle);
            Assert.Equal(0x00, v.SnapIsoPair);
            Assert.Equal(0, v.SnapRotation);
            Assert.Equal(DwgPoint.Origin, v.SnapBase);
            Assert.Equal(new DwgVector(0.5, 0.5, 0.0), v.SnapSpacing);
            Assert.Equal(new DwgHandle(0x08), v.ResolveHandleReference(v.ViewPortControlHandleReference));
        }

        [Fact]
        public void ReadRawAppIdControl()
        {
            var ac = (DwgAppIdControlObject)ParseRaw(
                0x0F, 0x00,                                     // length
                0x50, 0x80, 0x42, 0x64, 0x80, 0x00, 0x00, 0x09, // data
                0x02, 0x40, 0x30, 0x21, 0x11, 0x21, 0x86,
                0xFA, 0xD9                                      // crc
            );
            Assert.Equal(new DwgHandle(0x09), ac.Handle);
            Assert.Equal(2, ac._entityCount);
            Assert.Equal(new DwgHandle(0x11), ac.ResolveHandleReference(ac._entityHandleReferences[0]));
            Assert.Equal(new DwgHandle(0x86), ac.ResolveHandleReference(ac._entityHandleReferences[1]));
        }

        [Fact]
        public void ReadRawAppId()
        {
            var a = (DwgAppId)ParseRaw(
                0x13, 0x00,                                     // length
                0x50, 0xC0, 0x44, 0x67, 0x40, 0x00, 0x00, 0x09, // data
                0x04, 0x41, 0x43, 0x41, 0x44, 0xC0, 0x0C, 0x10,
                0x83, 0x05, 0x0A,
                0x8C, 0xE9                                      // crc
            );
            Assert.Equal(new DwgHandle(0x11), a.Handle);
            Assert.Equal("ACAD", a.Name);
            Assert.True(a._64flag);
            Assert.Equal(0, a._xrefIndex);
            Assert.False(a._isDependentOnXRef);
            Assert.Equal(0x00, a._unknown);
            Assert.Equal(new DwgHandle(0x09), a.ResolveHandleReference(a.AppIdControlHandleReference));
        }

        [Fact]
        public void ReadRawDimStyleControl()
        {
            var dc = (DwgDimStyleControlObject)ParseRaw(
                0x10, 0x00,                                     // length
                0x51, 0x00, 0x42, 0xA4, 0x80, 0x00, 0x00, 0x09, // data
                0x03, 0x40, 0x30, 0x21, 0x1D, 0x21, 0x4D, 0x20,
                0xBA, 0x14                                      // crc
            );
            Assert.Equal(new DwgHandle(0x0A), dc.Handle);
            Assert.Equal(3, dc._entityCount);
            Assert.Equal(new DwgHandle(0x1D), dc.ResolveHandleReference(dc._entityHandleReferences[0]));
            Assert.Equal(new DwgHandle(0x4D), dc.ResolveHandleReference(dc._entityHandleReferences[1]));
            Assert.Equal(new DwgHandle(0x00), dc.ResolveHandleReference(dc._entityHandleReferences[2]));
        }

        [Fact]
        public void ReadRawDimStyle()
        {
            var d = (DwgDimStyle)ParseRaw(
                0x70, 0x00,                                     // length
                0x51, 0x40, 0x47, 0x64, 0x90, 0x30, 0x00, 0x09, // data
                0x08, 0x53, 0x54, 0x41, 0x4E, 0x44, 0x41, 0x52,
                0x44, 0xC3, 0x00, 0x04, 0x00, 0x00, 0x80, 0x01,
                0x80, 0x00, 0x00, 0x00, 0x10, 0x29, 0x04, 0x41,
                0x10, 0x24, 0x09, 0x02, 0xB5, 0xE8, 0xDC, 0x0F,
                0x42, 0xB1, 0xCF, 0xC0, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x0B, 0x03, 0xF1, 0x4A, 0xE0, 0x7A, 0x17,
                0xAD, 0x47, 0x60, 0xFC, 0x0A, 0xD7, 0xA3, 0x70,
                0x3D, 0x0A, 0xC7, 0x3F, 0xAA, 0x02, 0xB5, 0xE8,
                0xDC, 0x0F, 0x42, 0xB1, 0xCF, 0xC0, 0xAD, 0x7A,
                0x37, 0x03, 0xD0, 0xAB, 0x73, 0xF8, 0x66, 0x66,
                0x66, 0x66, 0x66, 0x66, 0x39, 0x40, 0x64, 0x0A,
                0xD7, 0xA3, 0x70, 0x3D, 0x0A, 0xB7, 0x3F, 0xAA,
                0xAA, 0x20, 0x85, 0x18, 0x28, 0x28, 0x88, 0x00,
                0xCC, 0x33                                      // crc
            );
            Assert.Equal(new DwgHandle(0x1D), d.Handle);
            Assert.Equal("STANDARD", d.Name);
            Assert.True(d._64flag);
            Assert.Equal(0, d._xrefIndex);
            Assert.False(d._isDependentOnXRef);
            Assert.False(d.GenerateDimensionTolerances);
            Assert.False(d.GenerateDimensionLimits);
            Assert.True(d.DimensionTextInsideHorizontal);
            Assert.True(d.DimensionTextOutsideHorizontal);
            Assert.False(d.SuppressFirstDimensionExtensionLine);
            Assert.False(d.SuppressSecondDimensionExtensionLine);
            Assert.False(d.UseAlternateDimensioning);
            Assert.False(d.ForceDimensionLineExtensionsOutsideIfTextIs);
            Assert.False(d.UseSeparateArrowBlocksForDimensions);
            Assert.False(d.ForceDimensionTextInsideExtensions);
            Assert.False(d.SuppressOutsideExtensionDimensionLines);
            Assert.Equal(2, d.AlternateDimensioningDecimalPlaces);
            Assert.Equal(DwgUnitZeroSuppression.SuppressZeroFeetAndZeroInches, d.DimensionUnitZeroSuppression);
            Assert.False(d.DIMSD1);
            Assert.False(d.DIMSD2);
            Assert.Equal(DwgJustification.Middle, d.DimensionToleranceVerticalJustification);
            Assert.Equal(DwgDimensionTextJustification.AboveLineCenter, d.DimensionTextJustification);
            Assert.Equal(DwgDimensionFit.MoveEitherForBestFit, d.DimensionTextAndArrowPlacement);
            Assert.False(d.DimensionCursorControlsTextPosition);
            Assert.Equal(DwgUnitZeroSuppression.SuppressZeroFeetAndZeroInches, d.DimensionToleranceZeroSuppression);
            Assert.Equal(DwgUnitZeroSuppression.SuppressZeroFeetAndZeroInches, d.AlternateDimensioningZeroSupression);
            Assert.Equal(DwgUnitZeroSuppression.SuppressZeroFeetAndZeroInches, d.AlternateDimensioningToleranceZeroSupression);
            Assert.False(d.TextAboveDimensionLine);
            Assert.Equal(DwgUnitFormat.Decimal, d.DimensionUnitFormat);
            Assert.Equal(DwgAngleFormat.DecimalDegrees, d.DimensioningAngleFormat);
            Assert.Equal(4, d.DimensionUnitToleranceDecimalPlaces);
            Assert.Equal(4, d.DimensionToleranceDecimalPlaces);
            Assert.Equal(DwgUnitFormat.Decimal, d.AlternateDimensioningUnits);
            Assert.Equal(2, d.AlternateDimensioningToleranceDecimalPlaces);
            Assert.Equal(1.0, d.DimensioningScaleFactor);
            Assert.Equal(0.18, d.DimensioningArrowSize);
            Assert.Equal(0.0625, d.DimensionExtensionLineOffset);
            Assert.Equal(0.38, d.DimensionLineIncrement);
            Assert.Equal(0.18, d.DimensionExtensionLineExtension);
            Assert.Equal(0.0, d.DimensionDistanceRoundingValue);
            Assert.Equal(0.0, d.DimensionLineExtension);
            Assert.Equal(0.0, d.DimensionPlusTolerance);
            Assert.Equal(0.0, d.DimensionMinusTolerance);
            Assert.Equal(0.18, d.DimensioningTextHeight);
            Assert.Equal(0.09, d.CenterMarkSize);
            Assert.Equal(0.0, d.DimensioningTickSize);
            Assert.Equal(25.4, d.AlternateDimensioningScaleFactor);
            Assert.Equal(1.0, d.DimensionLinearMeasurementsScaleFactor);
            Assert.Equal(0.0, d.DimensionVerticalTextPosition);
            Assert.Equal(1.0, d.DimensionToleranceDisplayScaleFactor);
            Assert.Equal(0.09, d.DimensionLineGap);
            Assert.Equal("", d.DimensioningSuffix);
            Assert.Equal("", d.AlternateDimensioningSuffix);
            Assert.Equal("", d.ArrowBlockName);
            Assert.Equal("", d.FirstArrowBlockName);
            Assert.Equal("", d.SecondArrowBlockName);
            Assert.Equal(DwgColor.ByBlock, d.DimensionLineColor);
            Assert.Equal(DwgColor.ByBlock, d.DimensionExtensionLineColor);
            Assert.Equal(DwgColor.ByBlock, d.DimensionTextColor);
            Assert.False(d._unknown);
            Assert.Equal(new DwgHandle(0x0A), d.ResolveHandleReference(d.DimStyleControlHandleReference));
            Assert.Equal(new DwgHandle(0x10), d.ResolveHandleReference(d._styleHandleReference));
        }

        [Fact]
        public void ReadRawViewPortEntityHeaderControl()
        {
            var vc = (DwgViewPortEntityHeaderControlObject)ParseRaw(
                0x17, 0x00,                                     // length
                0x51, 0x80, 0x42, 0xE4, 0x80, 0x00, 0x00, 0x09, // data
                0x06, 0x40, 0x30, 0x21, 0x51, 0x21, 0x52, 0x21,
                0x54, 0x21, 0x56, 0x21, 0x58, 0x21, 0x5A,
                0x9E, 0x84                                      // crc
            );
            Assert.Equal(new DwgHandle(0x0B), vc.Handle);
            Assert.Equal(6, vc._entityCount);
            Assert.Equal(new DwgHandle(0x51), vc.ResolveHandleReference(vc._entityHandleReferences[0]));
            Assert.Equal(new DwgHandle(0x52), vc.ResolveHandleReference(vc._entityHandleReferences[1]));
            Assert.Equal(new DwgHandle(0x54), vc.ResolveHandleReference(vc._entityHandleReferences[2]));
            Assert.Equal(new DwgHandle(0x56), vc.ResolveHandleReference(vc._entityHandleReferences[3]));
            Assert.Equal(new DwgHandle(0x58), vc.ResolveHandleReference(vc._entityHandleReferences[4]));
            Assert.Equal(new DwgHandle(0x5A), vc.ResolveHandleReference(vc._entityHandleReferences[5]));
        }

        [Fact]
        public void ReadRawViewPortEntityHeader()
        {
            var v = (DwgViewPortEntityHeader)ParseRaw(
                0x11, 0x00,                                     // length
                0x51, 0xC0, 0x56, 0x24, 0x50, 0x00, 0x00, 0x0A, // data
                0xCA, 0x08, 0x59, 0x82, 0x82, 0x0A, 0xCA, 0x8A,
                0xB4,
                0x2F, 0x9E                                      // crc
            );
            Assert.Equal(new DwgHandle(0x58), v.Handle);
            Assert.Equal("", v.Name);
            Assert.True(v._64flag);
            Assert.Equal(0, v._xrefIndex);
            Assert.False(v._isDependentOnXRef);
            Assert.True(v._1flag);
            Assert.Equal(new DwgHandle(0x0B), v.ResolveHandleReference(v.ViewPortEntityHeaderControlHandleReference));
            Assert.Equal(new DwgHandle(0x59), v.ResolveHandleReference(v._nextViewPortEntityHeaderHandleReference));
        }

        [Fact]
        public void ReadRawGroup()
        {
            var g = (DwgGroup)ParseRaw(
               0x27, 0x00,                                     // length
               0x52, 0x00, 0x5E, 0xED, 0xE0, 0x00, 0x00, 0x04, // data
               0x05, 0x0F, 0x74, 0x68, 0x69, 0x73, 0x20, 0x69,
               0x73, 0x20, 0x6D, 0x79, 0x67, 0x72, 0x6F, 0x75,
               0x70, 0x90, 0x14, 0x0D, 0x04, 0x35, 0x04, 0x34,
               0xC1, 0x45, 0xE9, 0x45, 0xB5, 0x45, 0xA1,
               0x35, 0x69                                      // crc
            );
            Assert.Equal(new DwgHandle(0x7B), g.Handle);
            Assert.Equal("this is mygroup", g.Name);
            Assert.True(g.IsSelectable);
            Assert.Equal(3, g._handles.Count);
            Assert.Equal(new DwgHandle(0x0D), g.ResolveHandleReference(g._handles[0]));
            Assert.Equal(new DwgHandle(0x0D), g.ResolveHandleReference(g._handles[1]));
            Assert.Equal(new DwgHandle(0x00), g.ResolveHandleReference(g._handles[2]));
        }

        [Fact]
        public void ReadRawMLineStyle()
        {
            var m = (DwgMLineStyle)ParseRaw(
                0x55, 0x00,                                     // length
                0x52, 0x40, 0x5D, 0x27, 0xC0, 0x20, 0x00, 0x04, // data
                0x05, 0x09, 0x4D, 0x59, 0x4D, 0x4C, 0x53, 0x54,
                0x59, 0x4C, 0x45, 0x44, 0x9B, 0x5E, 0x48, 0x1B,
                0x5D, 0x5B, 0x1D, 0x1A, 0x5B, 0x1A, 0x5B, 0x99,
                0x48, 0x1C, 0xDD, 0x1E, 0x5B, 0x19, 0x68, 0x18,
                0x2D, 0x44, 0x54, 0xFB, 0x21, 0xF9, 0x3F, 0x06,
                0x0B, 0x51, 0x15, 0x3E, 0xC8, 0x7E, 0x4F, 0xC0,
                0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0E, 0x03,
                0xFD, 0x01, 0x90, 0x44, 0x08, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0xE0, 0xBF, 0x40, 0x90, 0x34,
                0x10, 0xE4, 0x10, 0xE3, 0x00,
                0x8F, 0xAA                                      // crc
            );
            Assert.Equal(new DwgHandle(0x74), m.Handle);
            Assert.Equal("MYMLSTYLE", m.Name);
            Assert.Equal("my multiline style", m.Description);
            Assert.Equal(0x00, m.Flags);
            Assert.Equal(DwgColor.ByBlock, m.FillColor);
            Assert.Equal(Math.PI / 2.0, m.StartAngle);
            Assert.Equal(Math.PI / 2.0, m.EndAngle);
            Assert.Equal(3, m._lineStyleCount);
            Assert.Equal(new double[] { 0.5, 0.0, -0.5 }, m._lineStyleOffsets);
            Assert.Equal(new short[] { 256, 4, 2 }, m._lineStyleColors);
            Assert.Equal(new short[] { 1, 2, 3 }, m._lineStyleLineTypeIndicies);
            Assert.Equal(new DwgHandle(0x0E), m.ResolveHandleReference(m._parentHandleReference));
        }

        [Fact]
        public void ReadRawDictionaryVar()
        {
            var d = (DwgDictionaryVar)ParseRaw(
                new[]
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    new DwgClassDefinition(0, 0, "", "", "DICTIONARYVAR", false, false),
                },
                0x12, 0x00,                                     // length
                0x3E, 0x40, 0x40, 0x80, 0x7A, 0xA7, 0x00, 0x00, // data
                0x00, 0x04, 0x04, 0x01, 0x01, 0x33, 0x40, 0x41,
                0xA2, 0x30,
                0xAC, 0xDA                                      // crc
            );
            Assert.Equal(new DwgHandle(0x01EA), d.Handle);
            Assert.Equal(0x00, d.IntVal);
            Assert.Equal("3", d.Str);
            Assert.Equal(new DwgHandle(0x00), d.ResolveHandleReference(d._parentHandleReference));
        }

        [Fact]
        public void ReadRawIdBuffer()
        {
            var id = (DwgIdBuffer)ParseRaw(
                0x12, 0x00,                                     // length
                0x3E, 0x80, 0x40, 0x62, 0xE6, 0x00, 0x00, 0x00, // data
                0x04, 0x04, 0x01, 0x01, 0x80, 0x41, 0x8A, 0x30,
                0x41, 0x89,
                0xC9, 0x64                                      // crc
            );
            Assert.Equal(new DwgHandle(0x8B), id.Handle);
            Assert.Equal(0x00, id._unknownRc);
            Assert.Single(id._objectHandleReferences);
            Assert.Equal(new DwgHandle(0x8A), id.ResolveHandleReference(id._objectHandleReferences[0]));
        }

        [Fact]
        public void ReadRawImage()
        {
            var i = (DwgImage)ParseRaw(
                0x09, 0x01,                                     // length
                0x3E, 0x40, 0x40, 0x5B, 0x6C, 0x60, 0x00, 0x00, // data
                0x04, 0x60, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00,
                0x04, 0x20, 0x00, 0x00, 0x00, 0x30, 0x00, 0x00,
                0x00, 0x28, 0x00, 0x00, 0x06, 0xB5, 0x6E, 0x62,
                0x0B, 0xAA, 0xE1, 0x02, 0x00, 0x03, 0x72, 0xC5,
                0x63, 0xF8, 0xD8, 0xAA, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x06, 0xB5, 0x6E, 0x62,
                0x0B, 0xAA, 0xE1, 0x02, 0x00, 0x03, 0x72, 0xC5,
                0x63, 0xF8, 0xD8, 0xCA, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x06, 0xB5, 0x6E, 0x62,
                0x0B, 0xAA, 0xE1, 0x12, 0x00, 0x03, 0x72, 0xC5,
                0x63, 0xF8, 0xD8, 0xCA, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x06, 0xB5, 0x6E, 0x62,
                0x0B, 0xAA, 0xE1, 0x12, 0x00, 0x03, 0x72, 0xC5,
                0x63, 0xF8, 0xD8, 0xAA, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x06, 0xB5, 0x6E, 0x62,
                0x0B, 0xAA, 0xE1, 0x02, 0x00, 0x03, 0x72, 0xC5,
                0x63, 0xF8, 0xD8, 0xAA, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x06, 0xD0, 0x38, 0x00,
                0x02, 0x80, 0xDB, 0x46, 0xB5, 0x6E, 0x62, 0x0B,
                0xAA, 0xE1, 0x02, 0x00, 0x00, 0xDC, 0xB1, 0x58,
                0xFE, 0x36, 0x2A, 0x81, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x10, 0x07, 0xF4, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x53, 0x10, 0x9E, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x10, 0x07, 0xF0, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x03, 0x02, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x03, 0x02, 0x02, 0x0E, 0x32,
                0x32, 0x00, 0x40, 0x40, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x38, 0x2F, 0xC0, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x38, 0x2F, 0xC0, 0x00, 0x00, 0x00, 0x00,
                0x38, 0x17, 0xD0, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x38, 0x17, 0xD0, 0x10, 0x5E, 0xCC, 0x14, 0x43,
                0xF0, 0x41, 0x68, 0x43, 0x54, 0x5A, 0xCC, 0x5B,
                0x3F,
                0x0D, 0x2A                                      // crc
            );
            Assert.Equal(new DwgHandle(0x6D), i.Handle);
            Assert.Equal(0, i.ClassVersion);
            Assert.Equal(new DwgPoint(8.180582100138889, 5.276854222199745, 0.0), i.InsertionPoint);
            Assert.Equal(new DwgVector(0.0078125, 0.0, 0.0), i.UVector);
            Assert.Equal(new DwgVector(4.783618569618661E-19, 0.0078125, 0.0), i.VVector);
            Assert.Equal(new DwgVector(128.0, 128.0, 0.0), i.ImageSize);
            Assert.True(i.ShowImage);
            Assert.True(i.ShowImageWhenNotAlignedToScreen);
            Assert.True(i.UseClippingBoundary);
            Assert.False(i.TransparencyOn);
            Assert.False(i.Clipping);
            Assert.Equal(50, i.Brightness);
            Assert.Equal(50, i.Contrast);
            Assert.Equal(0, i.Fade);
            Assert.Equal(DwgClipBoundaryType.Rectangle, i.ClipBoundaryType);
            Assert.Equal(new DwgPoint(-0.5, -0.5, 0.0), i.ClipBoundaryFirstPoint);
            Assert.Equal(new DwgPoint(127.5, 127.5, 0.0), i.ClipBoundarySecondPoint);
            Assert.Empty(i.ClipVertices);
            Assert.Equal(new DwgHandle(0x6B), i.ResolveHandleReference(i._imageDefHandleReference));
            Assert.Equal(new DwgHandle(0x6C), i.ResolveHandleReference(i._imageDefReactorHandleReference));
        }

        [Fact]
        public void ReadRawImageDefinition()
        {
            var id = (DwgImageDefinition)ParseRaw(
                0x4E, 0x00,                                     // length
                0x3D, 0xC0, 0x40, 0x5A, 0xE3, 0xB0, 0x20, 0x00, // data
                0x04, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x60, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x60, 0x40, 0x46, 0xD0, 0xCE, 0x97, 0x15, 0xD2,
                0x53, 0x93, 0x95, 0x17, 0x11, 0x99, 0x58, 0x5D,
                0x1A, 0x19, 0x5C, 0x95, 0x19, 0x5E, 0x1D, 0x1D,
                0x5C, 0x99, 0x4B, 0x98, 0x9B, 0x5C, 0x20, 0x5E,
                0x25, 0xD4, 0xEB, 0x07, 0x52, 0xBA, 0xC7, 0xFE,
                0x25, 0xD4, 0xEB, 0x07, 0x52, 0xBA, 0xC7, 0xF0,
                0x08, 0x2D, 0x48, 0x2D, 0x86, 0x11,
                0xE8, 0x23                                      // crc
            );
            Assert.Equal(new DwgHandle(0x6B), id.Handle);
            Assert.Equal(0, id.ClassVersion);
            Assert.Equal(128.0, id.ImageWidth);
            Assert.Equal(128.0, id.ImageHeight);
            Assert.Equal(@"C:\WINNT\FeatherTexture.bmp", id.FilePath);
            Assert.True(id.IsLoaded);
            Assert.Equal(DwgImageResolutionUnits.Centimeters, id.ResolutionUnits);
            Assert.Equal(0.352858149123434, id.PixelWidth);
            Assert.Equal(0.352858149123434, id.PixelHeight);
        }

        [Fact]
        public void ReadRawImageDefinitionReactor()
        {
            var i = (DwgImageDefinitionReactor)ParseRaw(
                0x0C, 0x00,                                     // length
                0x3E, 0x00, 0x40, 0x5B, 0x25, 0x00, 0x00, 0x00, // data
                0x09, 0x02, 0x60, 0x30,
                0xA1, 0x13                                      // crc
            );
            Assert.Equal(new DwgHandle(0x6C), i.Handle);
        }

        [Fact]
        public void ReadRawLayerIndex()
        {
            var i = (DwgLayerIndex)ParseRaw(
                0x59, 0x00,                                     // length
                0x3F, 0xC0, 0x40, 0x80, 0x7E, 0x20, 0xC0, 0x20, // data
                0x00, 0x04, 0x04, 0x61, 0x65, 0x25, 0x00, 0x3B,
                0x3A, 0x89, 0x80, 0x90, 0x64, 0xDD, 0x01, 0x30,
                0x42, 0x50, 0x64, 0x15, 0x34, 0x84, 0x14, 0x44,
                0x54, 0x05, 0x08, 0x55, 0x52, 0x4C, 0x4C, 0x41,
                0x59, 0x45, 0x52, 0x90, 0x94, 0x44, 0x54, 0x65,
                0x04, 0xF4, 0x94, 0xE5, 0x45, 0x34, 0x1D, 0x03,
                0x52, 0x45, 0x44, 0x41, 0x10, 0x44, 0x24, 0xC5,
                0x54, 0x58, 0x04, 0x20, 0x1F, 0x73, 0x03, 0x20,
                0x1F, 0xA3, 0x20, 0x1F, 0xB3, 0x20, 0x1F, 0xC3,
                0x20, 0x1F, 0xD3, 0x20, 0x1F, 0xE3, 0x20, 0x1F,
                0xFE,
                0x46, 0xE8                                      // crc
            );
            Assert.Equal(new DwgHandle(0x01F8), i.Handle);
            Assert.Equal(new DateTime(1997, 12, 2, 10, 1, 44, 940), i.TimeStamp);
            Assert.Equal(6, i._entryCount);
            Assert.Equal(55, i._layerIndices[0]);
            Assert.Equal(9, i._layerIndices[1]);
            Assert.Equal(1, i._layerIndices[2]);
            Assert.Equal(0, i._layerIndices[3]);
            Assert.Equal(7, i._layerIndices[4]);
            Assert.Equal(4, i._layerIndices[5]);
            Assert.Equal("0", i._layerNames[0]);
            Assert.Equal("ASHADE", i._layerNames[1]);
            Assert.Equal("URLLAYER", i._layerNames[2]);
            Assert.Equal("DEFPOINTS", i._layerNames[3]);
            Assert.Equal("RED", i._layerNames[4]);
            Assert.Equal("BLUE", i._layerNames[5]);
            Assert.Equal(new DwgHandle(0x01F7), i.ResolveHandleReference(i._parentHandleReference));
            Assert.Equal(new DwgHandle(0x01F7), i.ResolveHandleReference(i._reactorHandleReferences.Single()));
            Assert.Equal(new DwgHandle(0x01FA), i.ResolveHandleReference(i._layerHandleReferences[0]));
            Assert.Equal(new DwgHandle(0x01FB), i.ResolveHandleReference(i._layerHandleReferences[1]));
            Assert.Equal(new DwgHandle(0x01FC), i.ResolveHandleReference(i._layerHandleReferences[2]));
            Assert.Equal(new DwgHandle(0x01FD), i.ResolveHandleReference(i._layerHandleReferences[3]));
            Assert.Equal(new DwgHandle(0x01FE), i.ResolveHandleReference(i._layerHandleReferences[4]));
            Assert.Equal(new DwgHandle(0x01FF), i.ResolveHandleReference(i._layerHandleReferences[5]));
        }

        [Fact]
        public void ReadRawLwPolyline()
        {
            var p = (DwgLwPolyline)ParseRaw(
                0xC8, 0x00,                                     // length
                0x3E, 0xC0, 0x40, 0x80, 0x43, 0xD0, 0x35, 0x20, // data
                0x10, 0x94, 0x60, 0x10, 0x08, 0x2A, 0x0C, 0x00,
                0x00, 0x5E, 0xA0, 0x20, 0x80, 0x12, 0x14, 0x85,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0xA0,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0xA0,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x78, 0x1F,
                0x80, 0x00, 0x00, 0x00, 0x00, 0x40, 0x23, 0x20,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x78, 0x1F,
                0x80, 0x00, 0x00, 0x00, 0x00, 0x40, 0x23, 0x20,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x23, 0x20,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1E, 0x20,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x23, 0x20,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1E, 0xA0,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0xA0,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1E, 0xA0,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0xA0,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x20,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x20,
                0x55, 0x1F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFD,
                0xF7, 0xF5, 0x56, 0x08, 0x19, 0x82, 0x88, 0x7A,
                0x85, 0x93                                      // crc
            );
            Assert.Equal(new DwgHandle(0x010F), p.Handle);
            Assert.Equal(0.0, p.Width);
            Assert.Equal(0.0, p.Elevation);
            Assert.Equal(0.0, p.Thickness);
            Assert.Equal(DwgVector.ZAxis, p.Normal);

            // check vertex creation
            p.BindVertices();
            Assert.Equal(10, p.Vertices.Count);
            Assert.Equal(new DwgLwPolylineVertex(0.0, 0.0, 0.0, 0.0, 0.0), p.Vertices[0]);
            Assert.Equal(new DwgLwPolylineVertex(12.5, 0.0, 0.0, 0.0, 0.0), p.Vertices[1]);
            Assert.Equal(new DwgLwPolylineVertex(12.5, 1.0, 0.0, 0.0, 0.0), p.Vertices[2]);
            Assert.Equal(new DwgLwPolylineVertex(45.0, 1.0, 0.0, 0.0, 0.0), p.Vertices[3]);
            Assert.Equal(new DwgLwPolylineVertex(45.0, 2.0, 0.0, 0.0, -0.9999999999999999), p.Vertices[4]);
            Assert.Equal(new DwgLwPolylineVertex(45.0, 28.0, 0.0, 0.0, 0.0), p.Vertices[5]);
            Assert.Equal(new DwgLwPolylineVertex(45.0, 29.0, 0.0, 0.0, 0.0), p.Vertices[6]);
            Assert.Equal(new DwgLwPolylineVertex(12.5, 29.0, 0.0, 0.0, 0.0), p.Vertices[7]);
            Assert.Equal(new DwgLwPolylineVertex(12.5, 30.0, 0.0, 0.0, 0.0), p.Vertices[8]);
            Assert.Equal(new DwgLwPolylineVertex(0.0, 30.0, 0.0, 0.0, 0.0), p.Vertices[9]);

            // check parameter deconstruction (really a writing scenario, but this is too convenient to pass up)
            p.DeconstructVertices();
            Assert.True(p.HasBulges);
            Assert.False(p.HasWidths);
            Assert.Equal(10, p._pointCount);
            Assert.Equal(10, p._bulgeCount);
            Assert.Equal(0, p._widthCount);
            Assert.Equal(10, p._points.Count);
            Assert.Equal(Tuple.Create(0.0, 0.0), p._points[0]);
            Assert.Equal(Tuple.Create(12.5, 0.0), p._points[1]);
            Assert.Equal(Tuple.Create(12.5, 1.0), p._points[2]);
            Assert.Equal(Tuple.Create(45.0, 1.0), p._points[3]);
            Assert.Equal(Tuple.Create(45.0, 2.0), p._points[4]);
            Assert.Equal(Tuple.Create(45.0, 28.0), p._points[5]);
            Assert.Equal(Tuple.Create(45.0, 29.0), p._points[6]);
            Assert.Equal(Tuple.Create(12.5, 29.0), p._points[7]);
            Assert.Equal(Tuple.Create(12.5, 30.0), p._points[8]);
            Assert.Equal(Tuple.Create(0.0, 30.0), p._points[9]);
            Assert.Equal(0.0, p._bulges[0]);
            Assert.Equal(0.0, p._bulges[1]);
            Assert.Equal(0.0, p._bulges[2]);
            Assert.Equal(0.0, p._bulges[3]);
            Assert.Equal(-0.9999999999999999, p._bulges[4]);
            Assert.Equal(0.0, p._bulges[5]);
            Assert.Equal(0.0, p._bulges[6]);
            Assert.Equal(0.0, p._bulges[7]);
            Assert.Equal(0.0, p._bulges[8]);
            Assert.Equal(0.0, p._bulges[9]);
        }

        [Fact]
        public void ReadRawRasterVariables()
        {
            var r = (DwgRasterVariables)ParseRaw(
                0x11, 0x00,                                     // length
                0x3D, 0x40, 0x40, 0x56, 0xA6, 0x60, 0x00, 0x00, // data
                0x04, 0x06, 0x40, 0x50, 0x19, 0x01, 0x04, 0x30,
                0xC0,
                0xDC, 0xD2                                      // crc
            );
            Assert.Equal(new DwgHandle(0x5A), r.Handle);
            Assert.Equal(0, r.ClassVersion);
            Assert.True(r.DisplayFrame);
            Assert.True(r.IsHighQuality);
            Assert.Equal(DwgImageResolutionUnits.None, r.Units);
        }

        [Fact]
        public void ReadRawSortEntsTable()
        {
            var s = (DwgSortEntsTable)ParseRaw(
                new[]
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    new DwgClassDefinition(0, 0, "", "", "SORTENTSTABLE", false, false),
                },
                0x59, 0x00,                                     // length
                0x3E, 0x80, 0x40, 0x69, 0x67, 0x80, 0x10, 0x00, // data
                0x04, 0x05, 0x12, 0x01, 0x6E, 0x01, 0x68, 0x01,
                0x6C, 0x01, 0x5E, 0x01, 0x53, 0x01, 0x6A, 0x01,
                0x60, 0x01, 0x95, 0x01, 0x58, 0x01, 0xA6, 0x01,
                0x6F, 0x01, 0x6D, 0x01, 0x54, 0x01, 0x6B, 0x01,
                0x56, 0x01, 0x69, 0x01, 0x76, 0x01, 0x55, 0x40,
                0x41, 0xA4, 0x30, 0x41, 0x19, 0x41, 0x6D, 0x41,
                0x60, 0x41, 0x6B, 0x41, 0x56, 0x41, 0xA6, 0x41,
                0x69, 0x41, 0x58, 0x41, 0x76, 0x41, 0x54, 0x41,
                0x95, 0x41, 0x6E, 0x41, 0x6C, 0x41, 0x55, 0x41,
                0x6A, 0x41, 0x53, 0x41, 0x68, 0x41, 0x6F, 0x41,
                0x5E,
                0xD3, 0xA5                                      // crc
            );
            Assert.Equal(new DwgHandle(0xA5), s.Handle);
            Assert.Equal(new DwgHandle(0x00), s.ResolveHandleReference(s._parentHandleReference));
            Assert.Equal(new DwgHandle(0x19), s.ResolveHandleReference(s._ownerHandleReference));
            Assert.Equal(18, s._sortHandleReferences.Count);
            Assert.Equal(new DwgHandle(0x6E), s.ResolveHandleReference(s._sortHandleReferences[0]));
            Assert.Equal(new DwgHandle(0x68), s.ResolveHandleReference(s._sortHandleReferences[1]));
            Assert.Equal(new DwgHandle(0x6C), s.ResolveHandleReference(s._sortHandleReferences[2]));
            Assert.Equal(new DwgHandle(0x5E), s.ResolveHandleReference(s._sortHandleReferences[3]));
            Assert.Equal(new DwgHandle(0x53), s.ResolveHandleReference(s._sortHandleReferences[4]));
            Assert.Equal(new DwgHandle(0x6A), s.ResolveHandleReference(s._sortHandleReferences[5]));
            Assert.Equal(new DwgHandle(0x60), s.ResolveHandleReference(s._sortHandleReferences[6]));
            Assert.Equal(new DwgHandle(0x95), s.ResolveHandleReference(s._sortHandleReferences[7]));
            Assert.Equal(new DwgHandle(0x58), s.ResolveHandleReference(s._sortHandleReferences[8]));
            Assert.Equal(new DwgHandle(0xA6), s.ResolveHandleReference(s._sortHandleReferences[9]));
            Assert.Equal(new DwgHandle(0x6F), s.ResolveHandleReference(s._sortHandleReferences[10]));
            Assert.Equal(new DwgHandle(0x6D), s.ResolveHandleReference(s._sortHandleReferences[11]));
            Assert.Equal(new DwgHandle(0x54), s.ResolveHandleReference(s._sortHandleReferences[12]));
            Assert.Equal(new DwgHandle(0x6B), s.ResolveHandleReference(s._sortHandleReferences[13]));
            Assert.Equal(new DwgHandle(0x56), s.ResolveHandleReference(s._sortHandleReferences[14]));
            Assert.Equal(new DwgHandle(0x69), s.ResolveHandleReference(s._sortHandleReferences[15]));
            Assert.Equal(new DwgHandle(0x76), s.ResolveHandleReference(s._sortHandleReferences[16]));
            Assert.Equal(new DwgHandle(0x55), s.ResolveHandleReference(s._sortHandleReferences[17]));
            Assert.Equal(18, s._objectHandleReferences.Count);
            Assert.Equal(new DwgHandle(0x6D), s.ResolveHandleReference(s._objectHandleReferences[0]));
            Assert.Equal(new DwgHandle(0x60), s.ResolveHandleReference(s._objectHandleReferences[1]));
            Assert.Equal(new DwgHandle(0x6B), s.ResolveHandleReference(s._objectHandleReferences[2]));
            Assert.Equal(new DwgHandle(0x56), s.ResolveHandleReference(s._objectHandleReferences[3]));
            Assert.Equal(new DwgHandle(0xA6), s.ResolveHandleReference(s._objectHandleReferences[4]));
            Assert.Equal(new DwgHandle(0x69), s.ResolveHandleReference(s._objectHandleReferences[5]));
            Assert.Equal(new DwgHandle(0x58), s.ResolveHandleReference(s._objectHandleReferences[6]));
            Assert.Equal(new DwgHandle(0x76), s.ResolveHandleReference(s._objectHandleReferences[7]));
            Assert.Equal(new DwgHandle(0x54), s.ResolveHandleReference(s._objectHandleReferences[8]));
            Assert.Equal(new DwgHandle(0x95), s.ResolveHandleReference(s._objectHandleReferences[9]));
            Assert.Equal(new DwgHandle(0x6E), s.ResolveHandleReference(s._objectHandleReferences[10]));
            Assert.Equal(new DwgHandle(0x6C), s.ResolveHandleReference(s._objectHandleReferences[11]));
            Assert.Equal(new DwgHandle(0x55), s.ResolveHandleReference(s._objectHandleReferences[12]));
            Assert.Equal(new DwgHandle(0x6A), s.ResolveHandleReference(s._objectHandleReferences[13]));
            Assert.Equal(new DwgHandle(0x53), s.ResolveHandleReference(s._objectHandleReferences[14]));
            Assert.Equal(new DwgHandle(0x68), s.ResolveHandleReference(s._objectHandleReferences[15]));
            Assert.Equal(new DwgHandle(0x6F), s.ResolveHandleReference(s._objectHandleReferences[16]));
            Assert.Equal(new DwgHandle(0x5E), s.ResolveHandleReference(s._objectHandleReferences[17]));
        }

        [Fact]
        public void ReadRawSpatialFilter()
        {
            var s = (DwgSpatialFilter)ParseRaw(
                0x7B, 0x00,                                     // length
                0x3F, 0x40, 0x40, 0x80, 0x85, 0x6A, 0xA0, 0x30, // data
                0x00, 0x04, 0x05, 0x05, 0x96, 0xEA, 0x02, 0x5E,
                0x66, 0x70, 0x2E, 0x40, 0x3A, 0xAF, 0xB1, 0x4B,
                0x54, 0x7F, 0x16, 0x40, 0x27, 0xE0, 0xD7, 0x48,
                0x12, 0x9C, 0x30, 0x40, 0x4A, 0xF2, 0x5C, 0xDF,
                0x87, 0x03, 0x14, 0x40, 0xB5, 0xAB, 0x90, 0xF2,
                0x93, 0xF6, 0x31, 0x40, 0x82, 0x75, 0x1C, 0x3F,
                0x54, 0x3A, 0x17, 0x40, 0x75, 0x79, 0x73, 0xB8,
                0x56, 0xD7, 0x32, 0x40, 0xEF, 0x3D, 0x5C, 0x72,
                0xDC, 0x11, 0x20, 0x40, 0x74, 0x94, 0x83, 0xD9,
                0x04, 0x00, 0x2E, 0x40, 0xE7, 0xDF, 0x2E, 0xFB,
                0x75, 0xA7, 0x20, 0x40, 0xA6, 0xA4, 0x06, 0x9A,
                0x0F, 0x88, 0xC4, 0x46, 0xB0, 0x5D, 0x8A, 0x70,
                0x26, 0x06, 0xE1, 0x49, 0x2C, 0xDE, 0xA1, 0xC0,
                0x70, 0x29, 0x9A, 0xA6, 0xA9, 0x90, 0x10, 0x80,
                0x85, 0x0C, 0x10,
                0x07, 0x5E                                      // crc
            );
            Assert.Equal(new DwgHandle(0x0215), s.Handle);
            Assert.Equal(5, s.ClippingPoints.Count);
            Assert.Equal(new DwgPoint(15.219531, 5.624345, 0.0), s.ClippingPoints[0]);
            Assert.Equal(new DwgPoint(16.609654000000003, 5.003448000000001, 0.0), s.ClippingPoints[1]);
            Assert.Equal(new DwgPoint(17.963195000000002, 5.806962, 0.0), s.ClippingPoints[2]);
            Assert.Equal(new DwgPoint(18.841167000000002, 8.034885000000001, 0.0), s.ClippingPoints[3]);
            Assert.Equal(new DwgPoint(15.000036999999999, 8.327072, 0.0), s.ClippingPoints[4]);
            Assert.Equal(DwgVector.ZAxis, s.Extrusion);
            Assert.Equal(DwgPoint.Origin, s.ClipOrigin);
            Assert.True(s.DisplayBoundary);
            Assert.False(s.ClipFront);
            Assert.False(s.ClipBack);
            Assert.Equal(12, s.InverseBlockTransformationMatrix.Count);
            Assert.Equal(1.0, s.InverseBlockTransformationMatrix[0]);
            Assert.Equal(0.0, s.InverseBlockTransformationMatrix[1]);
            Assert.Equal(0.0, s.InverseBlockTransformationMatrix[2]);
            Assert.Equal(-12.731942, s.InverseBlockTransformationMatrix[3]);
            Assert.Equal(0.0, s.InverseBlockTransformationMatrix[4]);
            Assert.Equal(1.0, s.InverseBlockTransformationMatrix[5]);
            Assert.Equal(0.0, s.InverseBlockTransformationMatrix[6]);
            Assert.Equal(-2.191152, s.InverseBlockTransformationMatrix[7]);
            Assert.Equal(0.0, s.InverseBlockTransformationMatrix[8]);
            Assert.Equal(0.0, s.InverseBlockTransformationMatrix[9]);
            Assert.Equal(1.0, s.InverseBlockTransformationMatrix[10]);
            Assert.Equal(0.0, s.InverseBlockTransformationMatrix[11]);
            Assert.Equal(12, s.ClipBoundaryTransformationMatrix.Count);
            Assert.Equal(1.0, s.ClipBoundaryTransformationMatrix[0]);
            Assert.Equal(0.0, s.ClipBoundaryTransformationMatrix[1]);
            Assert.Equal(0.0, s.ClipBoundaryTransformationMatrix[2]);
            Assert.Equal(0.0, s.ClipBoundaryTransformationMatrix[3]);
            Assert.Equal(0.0, s.ClipBoundaryTransformationMatrix[4]);
            Assert.Equal(1.0, s.ClipBoundaryTransformationMatrix[5]);
            Assert.Equal(0.0, s.ClipBoundaryTransformationMatrix[6]);
            Assert.Equal(0.0, s.ClipBoundaryTransformationMatrix[7]);
            Assert.Equal(0.0, s.ClipBoundaryTransformationMatrix[8]);
            Assert.Equal(0.0, s.ClipBoundaryTransformationMatrix[9]);
            Assert.Equal(1.0, s.ClipBoundaryTransformationMatrix[10]);
            Assert.Equal(0.0, s.ClipBoundaryTransformationMatrix[11]);
            Assert.Equal(new DwgHandle(0x00), s.ResolveHandleReference(s._parentHandleReference));
        }
    }
}
