using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgHatch
    {
        public class PatternDefinitionLine
        {
            public double Angle { get; set; }
            public DwgPoint BasePoint { get; set; } = DwgPoint.Origin;
            public DwgVector Offset { get; set; } = DwgVector.Zero;
            public List<double> DashLengths { get; } = new List<double>();

            internal void Write(BitWriter writer)
            {
                writer.Write_BD(Angle);
                writer.Write_2BD(Converters.DoublePoint(BasePoint));
                writer.Write_2BD(Converters.DoubleVector(Offset));
                writer.Write_BS((short)DashLengths.Count);
                foreach (var dashLength in DashLengths)
                {
                    writer.Write_BD(dashLength);
                }
            }

            internal static PatternDefinitionLine Read(BitReader reader)
            {
                var result = new PatternDefinitionLine();
                result.Angle = reader.Read_BD();
                result.BasePoint = Converters.DoublePoint(reader.Read_2BD());
                result.Offset = Converters.DoubleVector(reader.Read_2BD());
                var dashCount = reader.Read_BS();
                for (int i = 0; i < dashCount; i++)
                {
                    result.DashLengths.Add(reader.Read_BD());
                }

                return result;
            }
        }
    }
}
