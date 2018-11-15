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

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache)
        {
            if (!LineTypeControlHandle.IsEmpty && LineTypeControlHandle.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect line type control object parent handle code.");
            }
        }

        internal override void ParseSpecific(BitReader reader, DwgVersionId version)
        {
            Handle = reader.Read_H();
            _xDataSize = reader.Read_BS();
            _xData = reader.Read_Bytes(_xDataSize);
            _objectSize = reader.Read_RL();
            _reactorCount = reader.Read_BL();
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
            LineTypeControlHandle = reader.Read_H();
            for (int i = 0; i < _reactorCount; i++)
            {
                _reactorHandles.Add(reader.Read_H());
            }
            _xDictionaryObjectHandle = reader.Read_H();
            _nullHandle = reader.Read_H();
        }

        internal override void WriteSpecific(BitWriter writer, DwgObjectMap objectMap, int pointerOffset, DwgVersionId version)
        {
            writer.Write_H(Handle);
            writer.Write_BS((short)_xDataSize);
            writer.Write_Bytes(_xData);
            writer.Write_RL(_objectSize);
            writer.Write_BL(_reactorHandles.Count);
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
            writer.Write_H(LineTypeControlHandle);
            foreach (var reactorHandle in _reactorHandles)
            {
                writer.Write_H(reactorHandle);
            }
            writer.Write_H(_xDictionaryObjectHandle);
            writer.Write_H(_nullHandle);
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
