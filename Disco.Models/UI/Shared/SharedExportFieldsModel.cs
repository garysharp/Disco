using Disco.Models.Services.Exporting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Disco.Models.UI.Shared
{
    public interface SharedExportFieldsModel<T> : BaseUIModel
        where T : IExportOptions
    {
        T Options { get; set; }
        List<ExportOptionGroup> FieldGroups { get; set; }
        void AddCustomUserDetails(Expression<Func<T, List<string>>> modelAccessor, int groupIndex = -1);
    }
}
