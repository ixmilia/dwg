using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace IxMilia.Dwg.Generator
{
    public abstract class GeneratorBase
    {
        private string _outputDir;
        private StringBuilder _sb;
        private bool _writingFile;
        private int _indentationLevel;

        protected GeneratorBase(string outputDir)
        {
            _outputDir = outputDir;
            _sb = new StringBuilder();
        }

        public void CreateNewFile(string ns, bool enableNullable, params string[] usings)
        {
            if (_writingFile)
            {
                throw new Exception($"File is still being written.  Please call `{nameof(FinishFile)}()` first.");
            }

            _sb = new StringBuilder();
            _writingFile = true;
            _indentationLevel = 0;

            if (enableNullable)
            {
                AppendLine("#nullable enable");
                AppendLine();
            }

            foreach (var u in usings)
            {
                AppendLine($"using {u};");
            }

            if (usings.Length > 0)
            {
                AppendLine();
            }

            AppendLine($"namespace {ns}");
            AppendLine("{");
        }

        public void IncreaseIndent()
        {
            _indentationLevel++;
        }

        public void DecreaseIndent()
        {
            _indentationLevel--;
        }

        public void AppendLine()
        {
            _sb.AppendLine();
        }

        public void AppendLine(string line)
        {
            var indentation = new string(' ', _indentationLevel * 4);
            _sb.Append(indentation);
            _sb.AppendLine(line);
        }

        public void FinishFile(string path)
        {
            AppendLine("}");
            var fullPath = Path.Combine(_outputDir, path);
            File.WriteAllText(fullPath, _sb.ToString());
            _sb = new StringBuilder();
            _writingFile = false;
        }

        // xml helpers
        public string? AttributeValue(XElement xml, string attributeName) => xml?.Attribute(attributeName)?.Value;
        public string AttributeValueNotNull(XElement xml, string attributeName) => AttributeValue(xml, attributeName) ?? throw new ArgumentNullException(nameof(attributeName));

        public string Accessibility(XElement xml, string defaultValue = "public") => AttributeValue(xml, "Accessibility") ?? defaultValue;

        public string? BaseClass(XElement xml) => AttributeValue(xml, "BaseClass");
        public string BinaryType(XElement xml) => AttributeValueNotNull(xml, "BinaryType");
        public int Code(XElement xml) => int.Parse(AttributeValueNotNull(xml, "Code"));
        public string? Comment(XElement xml) => AttributeValue(xml, "Comment");
        public string ConstructorAccessibility(XElement xml, string defaultValue = "public") => AttributeValue(xml, "ConstructorAccessibility") ?? defaultValue;
        public bool CustomReader(XElement xml) => bool.Parse(AttributeValue(xml, "CustomReader") ?? "false");
        public bool CustomWriter(XElement xml) => bool.Parse(AttributeValue(xml, "CustomWriter") ?? "false");
        public string DefaultValue(XElement xml) => AttributeValueNotNull(xml, "DefaultValue");
        public bool EnableNullable(XElement xml) => bool.Parse(AttributeValue(xml, "EnableNullable") ?? "false");
        public string From(XElement xml) => AttributeValueNotNull(xml, "From");
        public bool IsEntity(XElement xml) => bool.Parse(AttributeValue(xml, "IsEntity") ?? "false");
        public bool IsImplemented(XElement xml) => !bool.Parse(AttributeValue(xml, "NotImplemented") ?? "false");
        public string Mask(XElement xml) => AttributeValueNotNull(xml, "Mask");
        public string? Name(XElement xml) => AttributeValue(xml, "Name");
        public string? ReadCondition(XElement xml) => AttributeValue(xml, "ReadCondition");
        public string? ReadConverter(XElement xml) => AttributeValue(xml, "ReadConverter");
        public string? ReadCount(XElement xml) => AttributeValue(xml, "ReadCount");
        public string? ReaderArgument(XElement xml) => AttributeValue(xml, "ReaderArgument");
        public string? ShortName(XElement xml) => AttributeValue(xml, "ShortName");
        public bool SkipCreation(XElement xml) => bool.Parse(AttributeValue(xml, "SkipCreation") ?? "false");
        public string To(XElement xml) => AttributeValueNotNull(xml, "To");
        public string Type(XElement xml) => AttributeValueNotNull(xml, "Type");
        public string Value(XElement xml) => AttributeValueNotNull(xml, "Value");
        public string? WriteCondition(XElement xml) => AttributeValue(xml, "WriteCondition");
        public string? WriteConverter(XElement xml) => AttributeValue(xml, "WriteConverter");
        public string XDataConversion(XElement xml) => AttributeValue(xml, "XDataConversion") ?? ReadConverter(xml) ?? "{0}";

        public string ApplyReadConverter(XElement xml, string value)
        {
            var readConverter = ReadConverter(xml);
            if (!string.IsNullOrEmpty(readConverter))
            {
                value = string.Format(readConverter, value);
            }

            return value;
        }

        public string? ApplyWriteConverter(XElement xml, string? value)
        {
            var writeConverter = WriteConverter(xml);
            if (!string.IsNullOrEmpty(writeConverter))
            {
                value = string.Format(writeConverter, value);
            }

            return value;
        }

        public bool ReportTypeAsNotNull(string typeName)
        {
            switch (typeName)
            {
                case "string":
                    return true;
                default:
                    return false;
            }
        }
    }
}
