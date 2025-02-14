using Disco.Models.Services.Exporting;
using Disco.Models.UI.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Models.Shared
{
    public static class ExportFieldsModel
    {
        public static ExportFieldsModel<V> Create<V>(V options, params string[] ignoreProperties)
            where V : IExportOptions
        {
            return new ExportFieldsModel<V>(options, ignoreProperties);
        }
    }

    public class ExportFieldsModel<T> : SharedExportFieldsModel<T> where T : IExportOptions
    {
        public T Options { get; set; }
        public List<ExportOptionGroup> FieldGroups { get; set; }

        public ExportFieldsModel(T options, params string[] ignoreProperties)
        {
            FieldGroups = GetFields(options, ignoreProperties);
        }

        private static List<ExportOptionGroup> GetFields(T options, params string[] ignoreProperties)
        {
            var viewData = new ViewDataDictionary<IExportOptions>(options);
            var metaData = ModelMetadata.FromLambdaExpression(o => o, viewData);

            var properties = metaData.Properties
                .Where(p => p.ShortDisplayName != null && p.ModelType == typeof(bool) && !ignoreProperties.Contains(p.PropertyName));

            return properties
                .Select(p => new ExportOptionField()
                {
                    GroupName = p.ShortDisplayName,
                    Name = p.PropertyName,
                    DisplayName = p.DisplayName,
                    Description = p.Description,
                    Checked = (bool)p.Model,
                })
                .GroupBy(p => p.GroupName)
                .Select(g =>
                {
                    var group = new ExportOptionGroup(g.Key);
                    group.AddRange(g);
                    return group;
                })
                .ToList();
        }
    }
}
