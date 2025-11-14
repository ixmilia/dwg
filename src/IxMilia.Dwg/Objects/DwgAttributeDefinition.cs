#nullable enable

namespace IxMilia.Dwg.Objects
{
    public partial class DwgAttributeDefinition
    {
        public DwgAttributeDefinition(string defaultValue, string tag, string prompt)
            : this()
        {
            DefaultValue = defaultValue;
            Tag = tag;
            Prompt = prompt;
        }
    }
}
