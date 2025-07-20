using Disco.Models.Services.Interop.DiscoServices;
using Disco.Services.Interop.DiscoServices;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.Other;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Web.Areas.Config.Models.Plugins
{
    public class IndexViewModel
    {
        public List<PluginManifest> PluginManifests { get; set; }
        public PluginLibraryManifestV2 PluginLibrary { get; set; }

        private Dictionary<PluginManifest, Tuple<PluginLibraryItemV2, PluginLibraryItemReleaseV2>> _PluginUpdates;
        public Dictionary<PluginManifest, Tuple<PluginLibraryItemV2, PluginLibraryItemReleaseV2>> PluginUpdates
        {
            get
            {
                if (_PluginUpdates == null)
                {
                    if (PluginLibrary == null || PluginLibrary.Plugins == null || PluginLibrary.Plugins.Count == 0 ||
                        PluginManifests == null || PluginManifests.Count == 0)
                    {
                        _PluginUpdates = new Dictionary<PluginManifest, Tuple<PluginLibraryItemV2, PluginLibraryItemReleaseV2>>(); // No Updates
                    }
                    else
                    {
                        var libraryIncompatibility = PluginLibrary.LoadIncompatibilityData();

                        _PluginUpdates = PluginManifests
                            .Join(
                                PluginLibrary.Plugins,
                                manifest => manifest.Id,
                                libraryItem => libraryItem.Id,
                                (manifest, libraryItem) => Tuple.Create(manifest, libraryItem, libraryItem.LatestCompatibleRelease(libraryIncompatibility)),
                                StringComparer.OrdinalIgnoreCase)
                            .Where(i => i.Item3 != null && Version.Parse(i.Item3.Version) > i.Item1.Version)
                            .ToDictionary(i => i.Item1, i => Tuple.Create(i.Item2, i.Item3));
                    }
                }
                return _PluginUpdates;
            }
        }

        public Dictionary<string, List<PluginManifest>> PluginManifestsByCategory
        {
            get
            {
                if (PluginManifests.Count == 0)
                    return null;

                var pluginsByCategory = new Dictionary<string, List<PluginManifest>>(StringComparer.Ordinal);

                foreach (var pluginManifest in PluginManifests)
                {
                    var orderedFeatures = pluginManifest.Features.OrderBy(pf => pf.Id);
                    var primaryFeature = orderedFeatures.Where(pf => pf.PrimaryFeature).FirstOrDefault();
                    Type categoryType;

                    if (primaryFeature == null)
                        primaryFeature = orderedFeatures.FirstOrDefault();

                    if (primaryFeature == null)
                        categoryType = typeof(OtherFeature);
                    else
                        categoryType = primaryFeature.CategoryType;

                    var categoryName = Disco.Services.Plugins.Plugins.PluginFeatureCategoryDisplayName(categoryType);

                    if (!pluginsByCategory.TryGetValue(categoryName, out var categoryPlugins))
                    {
                        categoryPlugins = new List<PluginManifest>();
                        pluginsByCategory.Add(categoryName, categoryPlugins);
                    }
                    categoryPlugins.Add(pluginManifest);
                }

                return pluginsByCategory;
            }
        }
    }
}