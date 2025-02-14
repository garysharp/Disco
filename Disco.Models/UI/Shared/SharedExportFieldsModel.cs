using Disco.Models.Services.Exporting;
using System.Collections.Generic;

namespace Disco.Models.UI.Shared
{
    public interface SharedExportFieldsModel<T> : BaseUIModel
        where T : IExportOptions
    {
        T Options { get; set; }
        List<ExportOptionGroup> FieldGroups { get; set; }
    }
}
