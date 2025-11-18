using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace IxMilia.Dwg.Generator
{
    internal class HeaderVariablesGenerator : GeneratorBase
    {
        private XElement _xml;
        private IEnumerable<XElement> _variables;
        private IEnumerable<XElement> _aliases;

        public HeaderVariablesGenerator(string outputDir)
            : base(outputDir)
        {
            _xml = XDocument.Load(Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location)!, "Spec", "HeaderVariables.xml")).Root!;
            _variables = _xml.Elements("Variable");
            _aliases = _xml.Element("ShortNameAliases")!.Elements("Alias");
        }

        public void Run()
        {
            CreateNewFile("IxMilia.Dwg", "System", "System.Diagnostics.CodeAnalysis");

            IncreaseIndent();
            AppendLine("public partial class DwgHeaderVariables");
            AppendLine("{");
            IncreaseIndent();

            // property declarations
            foreach (var v in _variables)
            {
                var name = Name(v);
                if (!string.IsNullOrEmpty(name))
                {
                    AppendLine("/// <summary>");
                    AppendLine($"/// The {ShortName(v)} header variable.  {Comment(v)}");
                    AppendLine("/// </summary>");
                    AppendLine($"{Accessibility(v)} {Type(v)} {name} {{ get; set; }}");
                    AppendLine();
                }
            }

            // flags
            foreach (var v in _variables)
            {
                foreach (var f in v.Elements("Flag"))
                {
                    AppendLine("/// <summary>");
                    AppendLine($"/// {Comment(f)}");
                    AppendLine("/// </summary>");
                    AppendLine($"public bool {Name(f)}");
                    AppendLine("{");
                    IncreaseIndent();
                    AppendLine($"get => Converters.GetFlag(this.{Name(v)}, {Mask(f)});");
                    AppendLine("set");
                    AppendLine("{");
                    IncreaseIndent();
                    AppendLine($"var flags = this.{Name(v)};");
                    AppendLine($"Converters.SetFlag(value, ref flags, {Mask(f)});");
                    AppendLine($"this.{Name(v)} = flags;");
                    DecreaseIndent();
                    AppendLine("}");
                    DecreaseIndent();
                    AppendLine("}");
                    AppendLine();
                }
            }

            // indexer
            AppendLine("public object this[string name]");
            AppendLine("{");
            IncreaseIndent();
            AppendLine("get");
            AppendLine("{");
            IncreaseIndent();
            AppendLine("if (name.StartsWith(\"$\"))");
            AppendLine("{");
            IncreaseIndent();
            AppendLine("name = name.Substring(1);");
            DecreaseIndent();
            AppendLine("}");
            AppendLine();
            AppendLine("switch (name)");
            AppendLine("{");
            IncreaseIndent();
            foreach (var v in _variables)
            {
                var name = Name(v);
                var shortName = ShortName(v);
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(shortName))
                {
                    AppendLine($"case \"{shortName}\":");
                    IncreaseIndent();
                    AppendLine($"return this.{name};");
                    DecreaseIndent();
                }
            }
            foreach (var a in _aliases)
            {
                var fromName = From(a);
                var toName = To(a);
                AppendLine($"case \"{fromName}\":");
                IncreaseIndent();
                AppendLine($"return this.{toName};");
                DecreaseIndent();
            }
            AppendLine("default:");
            IncreaseIndent();
            AppendLine("throw new NotSupportedException();");
            DecreaseIndent();
            DecreaseIndent();
            AppendLine("}");
            DecreaseIndent();
            AppendLine("}");
            AppendLine("set");
            AppendLine("{");
            IncreaseIndent();
            AppendLine("if (name.StartsWith(\"$\"))");
            AppendLine("{");
            IncreaseIndent();
            AppendLine("name = name.Substring(1);");
            DecreaseIndent();
            AppendLine("}");
            AppendLine();
            AppendLine("switch (name)");
            AppendLine("{");
            IncreaseIndent();
            foreach (var v in _variables)
            {
                var name = Name(v);
                var shortName = ShortName(v);
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(shortName))
                {
                    AppendLine($"case \"{shortName}\":");
                    IncreaseIndent();
                    AppendLine($"this.{name} = ({Type(v)})value;");
                    AppendLine("break;");
                    DecreaseIndent();
                }
            }
            foreach (var a in _aliases)
            {
                var fromName = From(a);
                var toName = To(a);
                var target = _variables.Single(v => Name(v) == toName);
                AppendLine($"case \"{fromName}\":");
                IncreaseIndent();
                AppendLine($"this.{toName} = ({Type(target)})value;");
                AppendLine("break;");
                DecreaseIndent();
            }
            AppendLine("default:");
            IncreaseIndent();
            AppendLine("throw new NotSupportedException();");
            DecreaseIndent();
            DecreaseIndent();
            AppendLine("}");
            DecreaseIndent();
            AppendLine("}");
            DecreaseIndent();
            AppendLine("}");

            // .ctor
            AppendLine();
            AppendLine("public DwgHeaderVariables()");
            AppendLine("{");
            IncreaseIndent();
            AppendLine("SetDefaults();");
            DecreaseIndent();
            AppendLine("}");

            // init
            AppendLine();
            foreach (var v in _variables)
            {
                if (ReportPropertyAsNotNull(v))
                {
                    AppendLine($"[MemberNotNull(nameof({Name(v)}))]");
                }
            }

            AppendLine("public void SetDefaults()");
            AppendLine("{");
            IncreaseIndent();
            foreach (var v in _variables)
            {
                var name = Name(v);
                if (!string.IsNullOrEmpty(name))
                {
                    AppendLine($"this.{name} = {DefaultValue(v)};");
                }
            }

            DecreaseIndent();
            AppendLine("}");

            // reader
            AppendLine();
            AppendLine("internal void ReadVariables(BitReader reader, DwgVersionId version)");
            AppendLine("{");
            IncreaseIndent();
            foreach (var v in _variables)
            {
                var name = Name(v);
                var variable = string.IsNullOrEmpty(name)
                    ? "_"
                    : $"this.{name}";
                var value = ApplyReadConverter(v, $"reader.Read_{BinaryType(v)}()");
                AppendLine($"{variable} = {value};");
            }

            DecreaseIndent();
            AppendLine("}");

            // writer
            AppendLine();
            AppendLine("internal void WriteVariables(BitWriter writer, DwgVersionId version)");
            AppendLine("{");
            IncreaseIndent();
            foreach (var v in _variables)
            {
                var name = Name(v);
                var value = string.IsNullOrEmpty(name)
                    ? DefaultValue(v)
                    : $"this.{name}";
                value = ApplyWriteConverter(v, value);
                AppendLine($"writer.Write_{BinaryType(v)}({value});");
            }

            DecreaseIndent();
            AppendLine("}");

            DecreaseIndent();
            AppendLine("}");
            DecreaseIndent();

            FinishFile("DwgHeaderVariables.Generated.cs");
        }
    }
}
