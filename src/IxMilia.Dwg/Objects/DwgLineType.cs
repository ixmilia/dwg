using System;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgLineType
    {
        public IList<DwgLineTypeDashInfo> DashInfos = new List<DwgLineTypeDashInfo>();

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

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            if (!LineTypeControlHandleReference.IsEmpty && LineTypeControlHandleReference.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect line type control object parent handle code.");
            }
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

            public DwgLineTypeDashInfo(double dashLength, short shapeCode, DwgVector offset, double scale, double rotation, short shapeFlag)
            {
                DashLength = dashLength;
                ShapeCode = shapeCode;
                Offset = offset;
                Scale = scale;
                Rotation = rotation;
                ShapeFlag = shapeFlag;
            }

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
