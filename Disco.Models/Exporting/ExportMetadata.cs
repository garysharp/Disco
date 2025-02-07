using System.Collections.Generic;

namespace Disco.Models.Exporting
{
    public class ExportMetadata<T>
        : List<ExportMetadataField<T>> where T : IExportRecord
    {
        public List<string> IgnoreShortNames { get; } = new List<string>();
    }
}
