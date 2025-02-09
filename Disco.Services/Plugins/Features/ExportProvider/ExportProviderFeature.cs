using Disco.Models.Exporting;
using Disco.Models.Services.Exporting;
using Disco.Services.Exporting;

namespace Disco.Services.Plugins.Features.ExportProvider
{
    [PluginFeatureCategory(DisplayName = "Exporter")]
    public class ExportProviderFeature : PluginFeature
    {
        public void RegisterExportType<T, E, R>()
            where T : IExport<E, R>, new()
            where E : IExportOptions, new()
            where R : IExportRecord
        {
            SavedExports.RegisterExportType<T, E, R>();
        }
    }
}
