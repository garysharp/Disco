using Disco.Services.Plugins;
using System;

namespace Disco.Web.Areas.Config.Models.Plugins
{
    public class PluginConfigurationViewModel
    {
        public PluginManifest Manifest { get; set; }
        public Type PluginViewType { get; set; }
        public object PluginViewModel { get; set; }

        public PluginConfigurationViewModel(PluginConfigurationHandler.PluginConfigurationHandlerGetResponse response)
        {
            Manifest = response.Manifest;

            PluginViewType = response.ViewType;
            PluginViewModel = response.Model;
        }
    }
}