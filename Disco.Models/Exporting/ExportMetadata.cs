using Disco.Models.Services.Exporting;
using System.Collections.Generic;

namespace Disco.Models.Exporting
{
    public class ExportMetadata<O, R>
        : List<ExportMetadataField<R>>
        where O : IExportOptions
        where R : IExportRecord
    {
        public List<string> IgnoreGroupNames { get; } = new List<string>();
        public O Options { get; set; }

        public ExportMetadata(O options)
        {
            Options = options;
        }
    }
}
