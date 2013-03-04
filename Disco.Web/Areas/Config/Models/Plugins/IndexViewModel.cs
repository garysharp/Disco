using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Models.BI.Interop.Community;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.Other;

namespace Disco.Web.Areas.Config.Models.Plugins
{
    public class IndexViewModel
    {
        public List<PluginManifest> PluginManifests { get; set; }
        public PluginLibraryUpdateResponse Catalogue { get; set; }

        private Dictionary<PluginManifest, PluginLibraryItem> _PluginUpdates;
        public Dictionary<PluginManifest, PluginLibraryItem> PluginUpdates
        {
            get
            {
                if (_PluginUpdates == null)
                {
                    if (Catalogue == null || Catalogue.Plugins == null || Catalogue.Plugins.Count == 0 ||
                        PluginManifests == null || PluginManifests.Count == 0)
                    {
                        _PluginUpdates = new Dictionary<PluginManifest, PluginLibraryItem>(); // No Updates
                    }
                    else
                    {
                        _PluginUpdates = PluginManifests.Join((IEnumerable<PluginLibraryItem>)Catalogue.Plugins, manifest => manifest.Id, update => update.Id, (manifest, update) => new Tuple<PluginManifest, PluginLibraryItem>(manifest, update)).Where(i => Version.Parse(i.Item2.LatestVersion) > i.Item1.Version).ToDictionary(i => i.Item1, i => i.Item2);
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