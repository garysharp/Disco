using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace Disco.Services.Plugins
{
    public class PluginDefinition
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Author { get; private set; }
        public Version Version { get; private set; }

        public Type PluginType { get; private set; }
        public Type PluginCategoryType { get; private set; }

        public Dictionary<string, string> PluginReferenceAssemblies { get; private set; }
        public string PluginHostDirectory { get; private set; }

        public bool HasConfiguration { get; private set; }
        public bool HasWebController { get; private set; }

        public PluginDefinition(Plugin PluginInstance, string HostDirectory, Dictionary<string, string> ReferencedAssemblies = null)
        {
            Type pluginType = PluginInstance.GetType();

            if (string.IsNullOrWhiteSpace(PluginInstance.Id))
                throw new ArgumentNullException("PluginInstance.Id");
            if (string.IsNullOrWhiteSpace(PluginInstance.Name))
                throw new ArgumentNullException("PluginInstance.Name");
            if (string.IsNullOrWhiteSpace(PluginInstance.Author))
                throw new ArgumentNullException("PluginInstance.Author");
            if (string.IsNullOrWhiteSpace(HostDirectory))
                throw new ArgumentNullException("HostDirectory");
            if (PluginInstance.Version == null)
                throw new ArgumentNullException("PluginInstance.Version");
            if (PluginInstance.PluginCategoryType == null)
                throw new ArgumentNullException("PluginInstance.PluginCategoryType");
            if (!PluginInstance.PluginCategoryType.IsAssignableFrom(pluginType))
                throw new ArgumentException(string.Format("The plugin [{0}] does not inherit the Category [{1}]", pluginType.Name, PluginInstance.PluginCategoryType.Name), "PluginInstance.PluginCategoryType");

            this.Id = PluginInstance.Id;
            this.Name = PluginInstance.Name;
            this.Author = PluginInstance.Author;
            this.Version = PluginInstance.Version;
            this.PluginType = pluginType;
            this.PluginCategoryType = PluginInstance.PluginCategoryType;

            this.PluginHostDirectory = HostDirectory;

            if (ReferencedAssemblies != null && ReferencedAssemblies.Count > 0)
                this.PluginReferenceAssemblies = ReferencedAssemblies;

            // Determine (and Validate) if has Configuration
            IPluginConfiguration pluginConfiguration = PluginInstance as IPluginConfiguration;
            if (pluginConfiguration != null)
            {
                if (pluginConfiguration.ConfigurationViewType == null)
                    throw new ArgumentNullException("PluginInstance.ConfigurationViewType");
                if (!typeof(WebViewPage).IsAssignableFrom(pluginConfiguration.ConfigurationViewType))
                    throw new ArgumentException(string.Format("The plugin [{0}] ConfigurationViewType must inherit System.Web.Mvc.WebViewPage", pluginType.Name), "PluginInstance.ConfigurationViewType");

                this.HasConfiguration = true;
            }
            
            // Determine if has Web Controller
            IPluginWebController pluginWebController = PluginInstance as IPluginWebController;
            this.HasWebController = (pluginWebController != null);
        }

        public Plugin CreateInstance()
        {
            return (Plugin)Activator.CreateInstance(PluginType);
        }
        public PluginType CreateInstance<PluginType>()
        {
            if (typeof(PluginType).IsAssignableFrom(this.PluginType))
                return (PluginType)Activator.CreateInstance(this.PluginType);
            else
                throw new InvalidOperationException(string.Format("The plugin [{0}] cannot be cast into type [{1}]", this.PluginType.Name, typeof(PluginType).Name));
        }
    }
}
