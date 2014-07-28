using Disco.Models.Services.Interop.DiscoServices;
using Disco.Services.Interop.DiscoServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Web.Areas.Config.Models.Plugins
{
    public class InstallModel
    {
        public PluginLibraryManifestV2 Library { get; set; }

        public List<Tuple<string, List<Tuple<PluginLibraryItemV2, PluginLibraryItemReleaseV2>>>> AvailablePlugins
        {
            get
            {
                var incompatibility = Library.LoadIncompatibilityData();

                return Library.Plugins
                    .Select(p => Tuple.Create(p, p.LatestCompatibleRelease(incompatibility)))
                    .Where(p => p.Item2 != null)
                    .GroupBy(p => p.Item1.PrimaryFeatureCategory)
                    .Select(p => Tuple.Create(p.Key, p.OrderBy(r => r.Item1.Name).ToList()))
                    .OrderBy(g => g.Item1)
                    .ToList();
            }
        }
    }
}