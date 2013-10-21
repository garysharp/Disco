using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Services.Plugins;

namespace Disco.Web.Areas.Config.Models.Plugins
{
    public class PluginConfigurationViewModel
    {
        public PluginManifest Manifest { get; set; }
        public Type PluginViewType { get; set; }
        public object PluginViewModel { get; set; }

        public PluginConfigurationViewModel(PluginConfigurationHandler.PluginConfigurationHandlerGetResponse response)
        {
            this.Manifest = response.Manifest;

            this.PluginViewType = response.ViewType;
            this.PluginViewModel = response.Model;
        }
    }
}