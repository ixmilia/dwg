using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace IxMilia.Dwg.Generator
{
    public class ObjectGenerator : GeneratorBase
    {
        private XElement _xml;
        private IEnumerable<XElement> _objects;

        public ObjectGenerator(string outputDir)
            : base(outputDir)
        {
            _xml = XDocument.Load(Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location)!, "Spec", "Objects.xml")).Root!;
            _objects = _xml.Elements("Object").Where(IsImplemented);
        }

        public void Run()
        {
            OutputObjectTypeEnum();
            OutputObjectBaseClass();
            OutputObjectClasses();
        }

        private void OutputObjectTypeEnum()
        {
            CreateNewFile("IxMilia.Dwg.Objects", false);

            IncreaseIndent();

            AppendLine("public enum DwgObjectType : short");
            AppendLine("{");
            IncreaseIndent();
            foreach (var o in _objects)
            {
                AppendLine($"{Name(o)} = {Value(o)},");
            }
            DecreaseIndent();
            AppendLine("}");

            AppendLine();

            AppendLine("public static class DwgObjectTypeExtensions");
            AppendLine("{");
            IncreaseIndent();
            AppendLine("public static short? TypeCodeFromClassName(string className)");
            AppendLine("{");
            IncreaseIndent();
            AppendLine("switch (className.ToUpperInvariant())");
            AppendLine("{");
            IncreaseIndent();
            foreach (var o in _objects.Where(o => AttributeValue(o, "ClassName") != null))
            {
                AppendLine($"case \"{AttributeValue(o, "ClassName")}\":");
                AppendLine($"    return {Value(o)};");
            }
            AppendLine("default:");
            AppendLine("    return null;");
            DecreaseIndent();
            AppendLine("}");
            DecreaseIndent();
            AppendLine("}");

            AppendLine();

            AppendLine("public static string ClassNameFromTypeCode(DwgObjectType type)");
            AppendLine("{");
            IncreaseIndent();
            AppendLine("switch (type)");
            AppendLine("{");
            IncreaseIndent();
            foreach (var o in _objects.Where(o => AttributeValue(o, "ClassName") != null))
            {
                AppendLine($"case DwgObjectType.{Name(o)}:");
                AppendLine($"    return \"{AttributeValue(o, "ClassName")}\";");
            }
            AppendLine("default:");
            AppendLine("    return null;");
            DecreaseIndent();
            AppendLine("}");
            DecreaseIndent();
            AppendLine("}");

            AppendLine("public static DwgClassDefinition GetClassDefinitionForObjectType(DwgObjectType type)");
            AppendLine("{");
            IncreaseIndent();
            AppendLine("switch (type)");
            AppendLine("{");
            IncreaseIndent();
            foreach (var o in _objects.Where(o => AttributeValue(o, "CppClassName") != null))
            {
                AppendLine($"case DwgObjectType.{Name(o)}:");
                AppendLine($"    return new DwgClassDefinition(0, 0, \"ObjectDBX Classes\", \"{AttributeValue(o, "CppClassName")}\", ClassNameFromTypeCode(DwgObjectType.{Name(o)}), false, {(IsEntity(o) ? "true" : "false")});");
            }
            AppendLine("default:");
            AppendLine("    return null;");
            DecreaseIndent();
            AppendLine("}");
            DecreaseIndent();
            AppendLine("}");

            DecreaseIndent();
            AppendLine("}");
            DecreaseIndent();

            FinishFile("DwgObjectType.Generated.cs");
        }

        private void OutputObjectBaseClass()
        {
            CreateNewFile("IxMilia.Dwg.Objects", false, "System");

            IncreaseIndent();
            AppendLine("public abstract partial class DwgObject");
            AppendLine("{");
            IncreaseIndent();
            AppendLine("internal static DwgObject CreateObject(DwgObjectType type)");
            AppendLine("{");
            IncreaseIndent();
            AppendLine("switch (type)");
            AppendLine("{");
            IncreaseIndent();
            foreach (var o in _objects)
            {
                AppendLine($"case DwgObjectType.{Name(o)}:");
                IncreaseIndent();
                AppendLine($"return new Dwg{Name(o)}();");
                DecreaseIndent();
            }

            AppendLine("default:");
            IncreaseIndent();
            AppendLine("throw new InvalidOperationException(\"Unexpected object type.\");");
            DecreaseIndent();
            DecreaseIndent();
            AppendLine("}");
            DecreaseIndent();
            AppendLine("}");

            DecreaseIndent();
            AppendLine("}");
            DecreaseIndent();

            FinishFile("DwgObject.Generated.cs");
        }

        private void OutputObjectClasses()
        {
            foreach (var o in _objects)
            {
                var enableNullable = EnableNullable(o);
                CreateNewFile("IxMilia.Dwg.Objects", enableNullable, "System", "System.Collections.Generic");

                IncreaseIndent();
                var baseClass = BaseClass(o) ?? (IsEntity(o) ? "DwgEntity" : "DwgObject");
                AppendLine($"{Accessibility(o)} partial class Dwg{Name(o)} : {baseClass}");
                AppendLine("{");
                IncreaseIndent();

                // properties
                AppendLine($"public override DwgObjectType Type => DwgObjectType.{Name(o)};");
                foreach (var p in o.Elements("Property"))
                {
                    if (SkipCreation(p))
                    {
                        continue;
                    }

                    var type = Type(p);
                    if (ReadCount(p) != null)
                    {
                        type = $"List<{type}>";
                    }

                    var comment = Comment(p);
                    if (!string.IsNullOrEmpty(comment))
                    {
                        AppendLine("/// <summary>");
                        AppendLine($"/// {comment}");
                        AppendLine("/// </summary>");
                    }

                    AppendLine($"{Accessibility(p)} {type} {Name(p)} {{ get; set; }}");
                }

                AppendLine();

                // flags
                foreach (var p in o.Elements("Property"))
                {
                    var flags = p.Elements("Flag").ToList();
                    if (flags.Count > 0)
                    {
                        AppendLine($"// {Name(p)} flags");
                        AppendLine();

                        foreach (var f in flags)
                        {
                            AppendLine($"public bool {Name(f)}");
                            AppendLine("{");
                            IncreaseIndent();
                            AppendLine($"get => Converters.GetFlag(this.{Name(p)}, {Mask(f)});");
                            AppendLine("set");
                            AppendLine("{");
                            IncreaseIndent();
                            AppendLine($"var flags = this.{Name(p)};");
                            AppendLine($"Converters.SetFlag(value, ref flags, {Mask(f)});");
                            AppendLine($"this.{Name(p)} = flags;");
                            DecreaseIndent();
                            AppendLine("}");
                            DecreaseIndent();
                            AppendLine("}");
                            AppendLine();
                        }
                    }
                }

                // .ctor
                AppendLine($"{ConstructorAccessibility(o)} Dwg{Name(o)}()");
                AppendLine("{");
                IncreaseIndent();
                AppendLine("SetDefaults();");
                DecreaseIndent();
                AppendLine("}");
                AppendLine();

                // defaults
                AppendLine("internal void SetDefaults()");
                AppendLine("{");
                IncreaseIndent();
                foreach (var p in o.Elements("Property"))
                {
                    AppendLine($"{Name(p)} = {DefaultValue(p)};");
                }

                DecreaseIndent();
                AppendLine("}");
                AppendLine();

                // writing
                if (!CustomReader(o))
                {
                    var foundEndOfEntity = false;
                    AppendLine("internal override void WriteSpecific(BitWriter writer, DwgVersionId version)");
                    AppendLine("{");
                    IncreaseIndent();
                    foreach (var p in o.Elements())
                    {
                        switch (p.Name.LocalName)
                        {
                            case "Property":
                                var condition = WriteCondition(p);
                                if (condition != null)
                                {
                                    AppendLine($"if ({condition})");
                                    AppendLine("{");
                                    IncreaseIndent();
                                }

                                var readCount = ReadCount(p);
                                if (string.IsNullOrEmpty(readCount))
                                {
                                    var value = ApplyWriteConverter(p, Name(p));
                                    AppendLine($"writer.Write_{BinaryType(p)}({value});");
                                }
                                else
                                {
                                    var value = ApplyWriteConverter(p, $"{Name(p)}[i]");
                                    AppendLine($"for (int i = 0; i < {readCount}; i++)");
                                    AppendLine("{");
                                    IncreaseIndent();
                                    AppendLine($"writer.Write_{BinaryType(p)}({value});");
                                    DecreaseIndent();
                                    AppendLine("}");
                                }

                                if (condition != null)
                                {
                                    DecreaseIndent();
                                    AppendLine("}");
                                }
                                break;
                            case "ObjectSizeEnd":
                                if (foundEndOfEntity)
                                {
                                    throw new Exception($"Duplicate 'LastPropertyForObjectSize' attributes on object '{Name(o)}'.");
                                }

                                foundEndOfEntity = true;
                                AppendLine("_objectSize = writer.BitCount;");
                                break;
                            default:
                                throw new Exception($"Unsupported element '{p.Name.LocalName}' on object '{Name(o)}'");
                        }
                    }

                    if (!foundEndOfEntity)
                    {
                        throw new Exception($"Missing 'ObjectSizeEnd' element on object '{Name(o)}'");
                    }

                    DecreaseIndent();
                    AppendLine("}");
                    AppendLine();
                }

                // parsing
                if (!CustomWriter(o))
                {
                    AppendLine("internal override void ParseSpecific(BitReader reader, int objectBitOffsetStart, DwgVersionId version)");
                    AppendLine("{");
                    IncreaseIndent();
                    foreach (var p in o.Elements())
                    {
                        switch (p.Name.LocalName)
                        {
                            case "Property":
                                var condition = ReadCondition(p);
                                if (condition != null)
                                {
                                    AppendLine($"if ({condition})");
                                    AppendLine("{");
                                    IncreaseIndent();
                                }

                                var readCount = ReadCount(p);
                                var value = ApplyReadConverter(p, $"reader.Read_{BinaryType(p)}({ReaderArgument(p)})");
                                if (string.IsNullOrEmpty(readCount))
                                {
                                    AppendLine($"{Name(p)} = {value};");
                                }
                                else
                                {
                                    AppendLine($"for (int i = 0; i < {readCount}; i++)");
                                    AppendLine("{");
                                    IncreaseIndent();
                                    AppendLine($"{Name(p)}.Add({value});");
                                    DecreaseIndent();
                                    AppendLine("}");
                                }

                                if (condition != null)
                                {
                                    DecreaseIndent();
                                    AppendLine("}");
                                }
                                break;
                            case "ObjectSizeEnd":
                                AppendLine("AssertObjectSize(reader, objectBitOffsetStart);");
                                break;
                            default:
                                throw new Exception($"Unsupported element '{p.Name.LocalName}' on object '{Name(o)}'");
                        }
                    }

                    DecreaseIndent();
                    AppendLine("}");
                    AppendLine();
                }

                if (Name(o) == "DimStyle")
                {
                    // generate style xdata
                    AppendLine("/// <summary>Generates <see cref=\"DwgXDataItemList\"/> of the difference between the styles.  Result may be <see langword=\"null\"/>.</summary>");
                    AppendLine("public static DwgXDataItemList GenerateStyleDifferenceAsXData(DwgDimStyle primaryStyle, DwgDimStyle modifiedStyle)");
                    AppendLine("{");
                    IncreaseIndent();

                    AppendLine("var itemList = new DwgXDataItemList();");
                    AppendLine();

                    foreach (var p in o.Elements().Where(p => p.Attribute("Code") != null).OrderBy(p => Code(p)))
                    {
                        AppendLine($"if (primaryStyle.{Name(p)} != modifiedStyle.{Name(p)})");
                        AppendLine("{");
                        AppendLine($"    itemList.Add(new DwgXDataShort({Code(p)}));");
                        AppendLine($"    itemList.Add({XDataValueFromProperty(p, "modifiedStyle")});");
                        AppendLine("}");
                        AppendLine();
                    }

                    AppendLine("return itemList.Count > 0");
                    AppendLine("    ? new DwgXDataItemList() { new DwgXDataString(XDataStyleName), itemList }");
                    AppendLine("    : null;");

                    DecreaseIndent();
                    AppendLine("}");
                    AppendLine();

                    // clone
                    AppendLine("public DwgDimStyle Clone()");
                    AppendLine("{");
                    IncreaseIndent();

                    AppendLine("var other = new DwgDimStyle();");
                    foreach (var p in o.Elements("Property"))
                    {
                        AppendLine($"other.{Name(p)} = {Name(p)};");
                    }

                    AppendLine("return other;");

                    DecreaseIndent();
                    AppendLine("}");
                    AppendLine();

                    // apply style xdata
                    AppendLine("private bool ApplyStyleOverride(short code, DwgXDataItem item)");
                    AppendLine("{");
                    IncreaseIndent();

                    AppendLine("switch (code)");
                    AppendLine("{");
                    IncreaseIndent();

                    foreach (var p in o.Elements().Where(p => p.Attribute("Code") != null))
                    {
                        AppendLine($"case {Code(p)} when item is {XDataTypeFromCode(Code(p))} x:");
                        AppendLine($"    {Name(p)} = {string.Format(XDataConversion(p), "x.Value")};");
                        AppendLine("    return true;");
                    }

                    DecreaseIndent();
                    AppendLine("}");
                    AppendLine();
                    AppendLine("return false;");

                    DecreaseIndent();
                    AppendLine("}");
                    AppendLine();

                    // get variable
                    AppendLine("public object GetVariable(string name)");
                    AppendLine("{");
                    IncreaseIndent();
                    AppendLine("switch (name?.ToUpperInvariant())");
                    AppendLine("{");
                    IncreaseIndent();
                    foreach (var p in o.Elements().Where(p => p.Attribute("HeaderVariable") != null))
                    {
                        AppendLine($"case \"{AttributeValue(p, "HeaderVariable")}\":");
                        AppendLine($"    return {Name(p)};");
                    }
                    AppendLine("default:");
                    AppendLine("    return null;");
                    DecreaseIndent();
                    AppendLine("}");
                    DecreaseIndent();
                    AppendLine("}");
                    AppendLine();

                    // set variable
                    AppendLine("public void SetVariable(string name, object value)");
                    AppendLine("{");
                    IncreaseIndent();
                    AppendLine("switch (name?.ToUpperInvariant())");
                    AppendLine("{");
                    IncreaseIndent();
                    foreach (var p in o.Elements().Where(p => p.Attribute("HeaderVariable") != null))
                    {
                        AppendLine($"case \"{AttributeValue(p, "HeaderVariable")}\":");
                        AppendLine($"    {Name(p)} = ({Type(p)})value;");
                        AppendLine("    break;");
                    }
                    DecreaseIndent();
                    AppendLine("}");
                    DecreaseIndent();
                    AppendLine("}");
                    AppendLine();
                }

                DecreaseIndent();
                AppendLine("}");
                DecreaseIndent();

                FinishFile($"Dwg{Name(o)}.Generated.cs");
            }
        }

        private string XDataValueFromProperty(XElement property, string itemName)
        {
            var ctor = XDataTypeFromCode(Code(property));
            var writeConverter = WriteConverter(property) ?? "{0}";
            var convertedValue = string.Format(writeConverter, $"{itemName}.{Name(property)}");
            return $"new {ctor}({convertedValue})";
        }

        private static string XDataTypeFromCode(int code)
        {
            return code switch
            {
                _ when code >= 0 && code <= 9 => "DwgXDataString",
                _ when code >= 40 && code <= 59 => "DwgXDataReal",
                _ when code >= 70 && code <= 79 => "DwgXDataShort",
                _ when code >= 140 && code <= 159 => "DwgXDataReal",
                _ when code >= 170 && code <= 179 => "DwgXDataShort",
                _ when code >= 270 && code <= 279 => "DwgXDataShort",
                _ when code >= 280 && code <= 289 => "DwgXDataShort", // bool
                _ when code >= 340 && code <= 349 => "DwgXDataString",
                _ => $"/* code {code} */",
                //_ => throw new NotSupportedException(""),
            };
        }
    }
}
