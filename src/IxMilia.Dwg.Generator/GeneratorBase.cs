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

            AppendLine();
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

        public string BinaryType(XElement xml) => AttributeValue(xml, "BinaryType");
        public string Comment(XElement xml) => AttributeValue(xml, "Comment");
        public string DefaultValue(XElement xml) => AttributeValue(xml, "DefaultValue");
        public string From(XElement xml) => AttributeValue(xml, "From");
        public string Mask(XElement xml) => AttributeValue(xml, "Mask");
        public string Name(XElement xml) => AttributeValue(xml, "Name");
        public string ReadConverter(XElement xml) => AttributeValue(xml, "ReadConverter");
        public string ShortName(XElement xml) => AttributeValue(xml, "ShortName");
        public string To(XElement xml) => AttributeValue(xml, "To");
        public string Type(XElement xml) => AttributeValue(xml, "Type");
        public string WriteConverter(XElement xml) => AttributeValue(xml, "WriteConverter");
    }
}
