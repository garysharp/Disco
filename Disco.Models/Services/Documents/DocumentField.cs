using System.Collections.Generic;

namespace Disco.Models.Services.Documents
{
    public class DocumentField
    {
        public string Name { get; }
        public string Value { get; }
        public int Ordinal { get; }

        public DocumentFieldType Type { get; }
        public bool IsRequired { get; }
        public bool IsReadOnly { get; }

        public List<string> FixedValues { get; }

        public DocumentField(string name, string value, int ordinal, DocumentFieldType type, bool isRequired, bool isReadOnly, List<string> fixedValues)
        {
            Name = name;
            Value = value;
            Ordinal = ordinal;
            Type = type;
            IsRequired = isRequired;
            IsReadOnly = isReadOnly;
            FixedValues = fixedValues;
        }
    }
}
