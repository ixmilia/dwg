﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgLineType
    {
        // not sure why, but AutoCAD refuses to accept a line type with a description that's not exactly 47 characters long
        private const int DescriptionLength = 47;

        public IList<DwgLineTypeDashInfo> DashInfos { get; } = new List<DwgLineTypeDashInfo>();

        internal override DwgHandleReferenceCode ExpectedNullHandleCode => DwgHandleReferenceCode.SoftOwner;

        public DwgLineType(string name)
            : this()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Name cannot be null.");
            }

            Name = name;
        }

        internal override void ParseSpecific(BitReader reader, int objectBitOffsetStart, DwgVersionId version)
        {
            Name = reader.Read_T();
            _64flag = reader.Read_B();
            _xrefIndex = reader.Read_BS();
            _isDependentOnXRef = reader.Read_B();
            Description = reader.Read_T();
            PatternLength = reader.Read_BD();
            _alignment = (char)reader.Read_RC();
            _numDashes = reader.Read_RC();
            for (int i = 0; i < _numDashes; i++)
            {
                var dashInfo = DwgLineTypeDashInfo.Parse(reader);
                DashInfos.Add(dashInfo);
            }
            _stringData = reader.ReadBytes(256); // TODO: handle the string data
            AssertObjectSize(reader, objectBitOffsetStart);
            LineTypeControlHandleReference = reader.Read_H();
            for (int i = 0; i < _reactorCount; i++)
            {
                _reactorHandleReferences.Add(reader.Read_H());
            }
            _xDictionaryObjectHandleReference = reader.Read_H();
            _nullHandleReference = reader.Read_H();
        }

        internal override void OnBeforeObjectWrite(DwgVersionId version)
        {
            // normalize dash lengths
            for (int i = 0; i < DashInfos.Count; i++)
            {
                DashInfos[i] = DashInfos[i].WithLength(Math.Abs(DashInfos[i].DashLength));
                if (i % 2 == 1)
                {
                    DashInfos[i] = DashInfos[i].WithLength(DashInfos[i].DashLength * -1.0);
                }
            }

            PatternLength = DashInfos.Sum(d => Math.Abs(d.DashLength));

            // ensure description is the correct length
            Description = Description is null ? new string(' ', DescriptionLength) : Description.PadRight(DescriptionLength);
            Description = Description.Substring(0, DescriptionLength);
        }

        internal override void WriteSpecific(BitWriter writer, DwgVersionId version)
        {
            writer.Write_T(Name);
            writer.Write_B(_64flag);
            writer.Write_BS(_xrefIndex);
            writer.Write_B(_isDependentOnXRef);
            writer.Write_T(Description);
            writer.Write_BD(PatternLength);
            writer.Write_RC((byte)_alignment);
            writer.Write_RC((byte)DashInfos.Count);
            foreach (var dashInfo in DashInfos)
            {
                dashInfo.Write(writer);
            }
            writer.WriteBytes(_stringData); // TODO: handle the string data
            _objectSize = writer.BitCount;
            writer.Write_H(LineTypeControlHandleReference);
            foreach (var reactorHandle in _reactorHandleReferences)
            {
                writer.Write_H(reactorHandle);
            }
            writer.Write_H(_xDictionaryObjectHandleReference);
            writer.Write_H(_nullHandleReference);
        }

        public struct DwgLineTypeDashInfo
        {
            public double DashLength { get; }
            public short ShapeCode { get; }
            public DwgVector Offset { get; }
            public double Scale { get; }
            public double Rotation { get; }
            public short ShapeFlag { get; }

            public DwgLineTypeDashInfo(double sectionLength)
                : this(sectionLength, 0, DwgVector.Zero, 1.0, 0.0, 0)
            {
            }

            public DwgLineTypeDashInfo(double dashLength, short shapeCode, DwgVector offset, double scale, double rotation, short shapeFlag)
                : this()
            {
                DashLength = dashLength;
                ShapeCode = shapeCode;
                Offset = offset;
                Scale = scale;
                Rotation = rotation;
                ShapeFlag = shapeFlag;
            }

            internal DwgLineTypeDashInfo WithLength(double length) => new DwgLineTypeDashInfo(length, ShapeCode, Offset, Scale, Rotation, ShapeFlag);

            internal static DwgLineTypeDashInfo Parse(BitReader reader)
            {
                var dashLength = reader.Read_BD();
                var shapeCode = reader.Read_BS();
                var xOffset = reader.Read_RD();
                var yOffset = reader.Read_RD();
                var scale = reader.Read_BD();
                var rotation = reader.Read_BD();
                var shapeFlag = reader.Read_BS();
                return new DwgLineTypeDashInfo(dashLength, shapeCode, new DwgVector(xOffset, yOffset, 0.0), scale, rotation, shapeFlag);
            }

            internal void Write(BitWriter writer)
            {
                writer.Write_BD(DashLength);
                writer.Write_BS(ShapeCode);
                writer.Write_RD(Offset.X);
                writer.Write_RD(Offset.Y);
                writer.Write_BD(Scale);
                writer.Write_BD(Rotation);
                writer.Write_BS(ShapeFlag);
            }
        }
    }
}
