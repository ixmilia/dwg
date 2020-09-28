using System;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgMLineStyle
    {
        public DwgMLineStyle(string name)
            : this()
        {
            if(string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Name cannot be null.");
            }

            Name = name;
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache)
        {
        }

        internal override void WriteSpecific(BitWriter writer, DwgVersionId version)
        {
            writer.Write_T(Name);
            writer.Write_T(Description);
            writer.Write_BS(Flags);
            writer.Write_BS(FillColor.RawValue);
            writer.Write_BD(StartAngle);
            writer.Write_BD(EndAngle);
            writer.Write_RC(_lineStyleCount);
            for (int i = 0; i < _lineStyleCount; i++)
            {
                writer.Write_BD(_lineStyleOffsets[i]);
                writer.Write_BS(_lineStyleColors[i]);
                writer.Write_BS(_lineStyleLineTypeIndicies[i]);
            }
            _objectSize = writer.BitCount;
            writer.Write_H(_parentHandle);
            for (int i = 0; i < _reactorCount; i++)
            {
                writer.Write_H(_reactorHandles[i]);
            }
            writer.Write_H(_xDictionaryObjectHandle);
        }

        internal override void ParseSpecific(BitReader reader, int objectBitOffsetStart, DwgVersionId version)
        {
            Name = reader.Read_T();
            Description = reader.Read_T();
            Flags = reader.Read_BS();
            FillColor = DwgColor.FromRawValue(reader.Read_BS());
            StartAngle = reader.Read_BD();
            EndAngle = reader.Read_BD();
            _lineStyleCount = reader.Read_RC();
            for (int i = 0; i < _lineStyleCount; i++)
            {
                _lineStyleOffsets.Add(reader.Read_BD());
                _lineStyleColors.Add(reader.Read_BS());
                _lineStyleLineTypeIndicies.Add(reader.Read_BS());
            }
            AssertObjectSize(reader, objectBitOffsetStart);
            _parentHandle = reader.Read_H();
            for (int i = 0; i < _reactorCount; i++)
            {
                _reactorHandles.Add(reader.Read_H());
            }
            _xDictionaryObjectHandle = reader.Read_H();
        }

        internal static DwgMLineStyle GetDefaultMLineStyle()
        {
            return new DwgMLineStyle("Standard");
        }
    }
}
