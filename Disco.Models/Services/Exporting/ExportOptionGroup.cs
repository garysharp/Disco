using System.Collections.Generic;

namespace Disco.Models.Services.Exporting
{
    public class ExportOptionGroup : List<ExportOptionField>
    {
        public string Name { get; set; }

        public ExportOptionGroup(string name)
        {
            Name = name;
        }
    }
}
