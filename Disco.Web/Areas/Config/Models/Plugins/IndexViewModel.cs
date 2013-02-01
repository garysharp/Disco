using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.Other;

namespace Disco.Web.Areas.Config.Models.Plugins
{
    public class IndexViewModel
    {
        public List<PluginManifest> PluginManifests { get; set; }

        public List<Tuple<Type, List<PluginManifest>>> PluginManifestsByType
        {
            get
            {
                if (PluginManifests.Count == 0)
                    return new List<Tuple<Type, List<PluginManifest>>>();

                var categorisedManifests = PluginManifests.SelectMany(pm => pm.Features)
                    .GroupBy(fm => fm.CategoryType)
                    .Select(g => new Tuple<Type, List<PluginManifest>>(g.Key, g.Select(fm => fm.PluginManifest).Distinct().OrderBy(fm => fm.Name).ToList())).ToList();
                
                // Ensure all plugins are represented
                var allCategorisedManifests = categorisedManifests.SelectMany(g => g.Item2).ToList();

                var unrepresentedPlugins = PluginManifests.Where(m => !allCategorisedManifests.Contains(m)).ToList();
                if (unrepresentedPlugins.Count > 0)
                {
                    Tuple<Type, List<PluginManifest>> otherCategory = null;
                    foreach (var category in categorisedManifests)
                    {
                        if (category.Item1 == typeof(OtherFeature))
                        {
                            otherCategory = category;
                        }
                    }
                    if (otherCategory == null)
                    {
                        otherCategory = new Tuple<Type, List<PluginManifest>>(typeof(OtherFeature), new List<PluginManifest>());
                        categorisedManifests.Add(otherCategory);
                    }
                    foreach (var pluginManifest in unrepresentedPlugins)
                        otherCategory.Item2.Add(pluginManifest);
                }

                return categorisedManifests;
            }
        }
    }
}