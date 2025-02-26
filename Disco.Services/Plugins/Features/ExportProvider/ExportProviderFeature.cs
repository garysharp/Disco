using Disco.Models.Exporting;
using Disco.Models.Services.Exporting;
using Disco.Services.Exporting;

namespace Disco.Services.Plugins.Features.ExportProvider
{
    [PluginFeatureCategory(DisplayName = "Exporter")]
    public class ExportProviderFeature : PluginFeature
    {
        public void RegisterExportType<T, O, R>()
            where T : IExport<O, R>, new()
            where O : IExportOptions, new()
            where R : IExportRecord
        {
            SavedExports.RegisterExportType<T, O, R>();
        }
    }
}
