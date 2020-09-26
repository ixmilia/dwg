using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace IxMilia.Dwg.Generator
{
    public abstract class GeneratorBase
    {
        private StringBuilder _sb;
        private int _indentationLevel;

        public void CreateNewFile(string ns, params string[] usings)
        {
            if (_sb != null)
            {
                throw new Exception($"File is still being written.  Please call `{nameof(FinishFile)}()` first.");
            }

            _sb = new StringBuilder();
            _indentationLevel = 0;

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
            File.WriteAllText(path, _sb.ToString());
            _sb = null;
        }

        // xml helpers
        public string AttributeValue(XElement xml, string attributeName) => xml?.Attribute(attributeName)?.Value;

        public string Accessibility(XElement xml, string defaultValue = "public") => AttributeValue(xml, "Accessibility") ?? defaultValue;

        public string BaseClass(XElement xml) => AttributeValue(xml, "BaseClass");
        public string BinaryType(XElement xml) => AttributeValue(xml, "BinaryType");
        public string Comment(XElement xml) => AttributeValue(xml, "Comment");
        public string ConstructorAccessibility(XElement xml, string defaultValue = "public") => AttributeValue(xml, "ConstructorAccessibility") ?? defaultValue;
        public bool CustomReader(XElement xml) => bool.Parse(AttributeValue(xml, "CustomReader") ?? "false");
        public bool CustomWriter(XElement xml) => bool.Parse(AttributeValue(xml, "CustomWriter") ?? "false");
        public string DefaultValue(XElement xml) => AttributeValue(xml, "DefaultValue");
        public string From(XElement xml) => AttributeValue(xml, "From");
        public bool IsEntity(XElement xml) => bool.Parse(AttributeValue(xml, "IsEntity") ?? "false");
        public bool IsImplemented(XElement xml) => !bool.Parse(AttributeValue(xml, "NotImplemented") ?? "false");
        public bool LastPropertyForObjectSize(XElement xml) => bool.Parse(AttributeValue(xml, "LastPropertyForObjectSize") ?? "false");
        public string Mask(XElement xml) => AttributeValue(xml, "Mask");
        public string Name(XElement xml) => AttributeValue(xml, "Name");
        public string ReadCondition(XElement xml) => AttributeValue(xml, "ReadCondition");
        public string ReadConverter(XElement xml) => AttributeValue(xml, "ReadConverter");
        public string ReadCount(XElement xml) => AttributeValue(xml, "ReadCount");
        public string ReaderArgument(XElement xml) => AttributeValue(xml, "ReaderArgument");
        public string ShortName(XElement xml) => AttributeValue(xml, "ShortName");
        public bool SkipCreation(XElement xml) => bool.Parse(AttributeValue(xml, "SkipCreation") ?? "false");
        public string To(XElement xml) => AttributeValue(xml, "To");
        public string Type(XElement xml) => AttributeValue(xml, "Type");
        public string Value(XElement xml) => AttributeValue(xml, "Value");
        public string WriteCondition(XElement xml) => AttributeValue(xml, "WriteCondition");
        public string WriteConverter(XElement xml) => AttributeValue(xml, "WriteConverter");

        public string ApplyReadConverter(XElement xml, string value)
        {
            var readConverter = ReadConverter(xml);
            if (!string.IsNullOrEmpty(readConverter))
            {
                value = string.Format(readConverter, value);
            }

            return value;
        }

        public string ApplyWriteConverter(XElement xml, string value)
        {
            var writeConverter = WriteConverter(xml);
            if (!string.IsNullOrEmpty(writeConverter))
            {
                value = string.Format(writeConverter, value);
            }

            return value;
        }
    }
}
