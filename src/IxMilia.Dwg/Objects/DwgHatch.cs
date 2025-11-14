#nullable enable

using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgHatch
    {
        public List<BoundaryPathBase> BoundaryPaths { get; } = new List<BoundaryPathBase>();
        public List<PatternDefinitionLine> PatternDefinitionLines { get; } = new List<PatternDefinitionLine>();
        public List<DwgPoint> SeedPoints { get; } = new List<DwgPoint>();

        private List<int> _boundaryObjectHandleCounts = new List<int>();

        internal override void ParseSpecific(BitReader reader, int objectBitOffsetStart, DwgVersionId version)
        {
            Elevation = reader.Read_BD();
            Extrusion = Converters.TripleVector(reader.Read_3BD());
            Name = reader.Read_T();
            IsSolidFill = reader.Read_B();
            IsAssociative = reader.Read_B();

            var anyPathFlagsWith4 = false;

            // read boundary path data
            var pathCount = reader.Read_BL();
            for (int i = 0; i < pathCount; i++)
            {
                var pathFlag = reader.Read_BL();
                anyPathFlagsWith4 |= Converters.GetFlag(pathFlag, 0x04);
                if (!Converters.GetFlag(pathFlag, 0x02))
                {
                    BoundaryPaths.Add(NonPolylineBoundaryPath.Read(reader));
                }
                else
                {
                    BoundaryPaths.Add(PolylineBoundaryPath.Read(reader));
                }

                _boundaryObjectHandleCounts.Add(reader.Read_BL());
            }

            // read pattern data
            HatchStyle = (DwgHatchStyle)reader.Read_BS();
            PatternType = (DwgHatchPatternType)reader.Read_BS();
            if (!IsSolidFill)
            {
                PatternAngle = reader.Read_BD();
                PatternScale = reader.Read_BD();
                IsPatternDoubled = reader.Read_B();
                var defLineCount = reader.Read_BS();
                PatternDefinitionLines.Clear();
                for (int i = 0; i < defLineCount; i++)
                {
                    PatternDefinitionLines.Add(PatternDefinitionLine.Read(reader));
                }
            }

            if (anyPathFlagsWith4)
            {
                PixelSize = reader.Read_BD();
            }

            var seedPointCount = reader.Read_BL();
            for (int i = 0; i < seedPointCount; i++)
            {
                SeedPoints.Add(Converters.DoublePoint(reader.Read_2RD()));
            }

            AssertObjectSize(reader, objectBitOffsetStart);
        }

        internal override void ReadPostData(BitReader reader, DwgVersionId version)
        {
            foreach (var boundarObjectHandleCount in _boundaryObjectHandleCounts)
            {
                for (int i = 0; i < boundarObjectHandleCount; i++)
                {
                    BoundaryPaths[i].BoundaryItemHandleReferences.Add(reader.Read_H());
                }
            }
        }

        internal override void WriteSpecific(BitWriter writer, DwgVersionId version)
        {
            writer.Write_BD(Elevation);
            writer.Write_3BD(Converters.TripleVector(Extrusion));
            writer.Write_T(Name);
            writer.Write_B(IsSolidFill);
            writer.Write_B(IsAssociative);
            writer.Write_BL(BoundaryPaths.Count);
            foreach (var path in BoundaryPaths)
            {
                var flag = path is PolylineBoundaryPath ? 0x02 : 0x00;
                writer.Write_BL(flag);
                path.Write(writer);
                writer.Write_BL(path.BoundaryItemHandleReferences.Count);
            }

            writer.Write_BS((short)HatchStyle);
            writer.Write_BS((short)PatternType);
            if (!IsSolidFill)
            {
                writer.Write_BD(PatternAngle);
                writer.Write_BD(PatternScale);
                writer.Write_B(IsPatternDoubled);
                writer.Write_BS((short)PatternDefinitionLines.Count);
                foreach (var line in PatternDefinitionLines)
                {
                    line.Write(writer);
                }
            }

            // not writing pixel size

            writer.Write_BL(SeedPoints.Count);
            foreach (var point in SeedPoints)
            {
                writer.Write_2RD(Converters.DoublePoint(point));
            }

            _objectSize = writer.BitCount;
        }

        internal override void WritePostData(BitWriter writer, DwgVersionId version)
        {
            foreach (var path in BoundaryPaths)
            {
                foreach (var handle in path.BoundaryItemHandleReferences)
                {
                    writer.Write_H(handle);
                }
            }
        }
    }
}
