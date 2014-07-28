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
                        _PluginUpdates = new Dictionary<PluginManifest,Tuple<PluginLibraryItemV2,PluginLibraryItemReleaseV2>>(); // No Updates
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

        public List<Tuple<Type, List<PluginManifest>>> PluginManifestsByCategory
        {
            get
            {
                if (PluginManifests.Count == 0)
                    return null;


                List<Tuple<Type, PluginManifest>> pluginsByCategory = new List<Tuple<Type, PluginManifest>>();

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

                    pluginsByCategory.Add(new Tuple<Type, PluginManifest>(categoryType, pluginManifest));
                }

                return pluginsByCategory.GroupBy(p => p.Item1)
                    .OrderBy(g => g.Key.Name)
                    .Select(g => new Tuple<Type, List<PluginManifest>>(g.Key, g.Select(pg => pg.Item2).OrderBy(p => p.Name).ToList())).ToList();
            }
        }
    }
}