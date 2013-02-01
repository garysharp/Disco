using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disco.Data.Repository;
using Newtonsoft.Json;
using System.Reflection;

namespace Disco.Services.Plugins
{
    public class PluginFeatureManifest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        
        [JsonProperty]
        private string CategoryTypeName { get; set; }

        [JsonIgnore]
        public PluginManifest PluginManifest { get; private set; }
        [JsonIgnore]
        internal Type Type { get; private set; }
        [JsonIgnore]
        public Type CategoryType { get; private set; }

        internal bool Initialize(DiscoDataContext dbContext, PluginManifest pluginManifest)
        {
            this.PluginManifest = pluginManifest;

            if (this.Type == null)
                this.Type = this.PluginManifest.PluginAssembly.GetType(this.TypeName, true, true);

            if (this.CategoryType == null)
                this.CategoryType = Type.GetType(this.CategoryTypeName, true, true);

            using (var instance = this.CreateInstance())
            {
                instance.Initalize(dbContext);
            }

            PluginsLog.LogInitializedPluginFeature(this.PluginManifest, this);

            return true;
        }

        public PluginFeature CreateInstance()
        {
            var i = (PluginFeature)Activator.CreateInstance(Type);
            i.Manifest = this;
            return i;
        }
        public CategoryType CreateInstance<CategoryType>() where CategoryType : PluginFeature
        {
            if (typeof(CategoryType).IsAssignableFrom(this.Type))
            {
                var i = (CategoryType)Activator.CreateInstance(Type);
                i.Manifest = this;
                return i;
            }
            else
                throw new InvalidOperationException(string.Format("The feature [{0}] cannot be cast into type [{1}]", this.Type.Name, typeof(CategoryType).Name));
        }

        /// <summary>
        /// Uses reflection to build a Plugin Manifest
        /// </summary>
        /// <param name="pluginAssembly">Assembly containing a plugin</param>
        /// <returns>A plugin manifest for the first encountered plugin within the assembly</returns>
        public static PluginFeatureManifest FromPluginFeatureType(Type featureType, PluginManifest pluginManifest)
        {
            var featureAttribute = (PluginFeatureAttribute)featureType.GetCustomAttributes(typeof(PluginFeatureAttribute), false).FirstOrDefault();

            if (featureAttribute == null)
                throw new ArgumentException(string.Format("Plugin Feature found [{0}], but no PluginFeatureAttribute found", featureType.Name), "featureType");

            var featureId = featureAttribute.Id;
            var featureName = featureAttribute.Name;

            // Determine Feature Category
            var featureCategoryType = featureType.BaseType;

            if (featureCategoryType == null)
                throw new ArgumentException(string.Format("Plugin Feature found [{0}], but has no Base Type to determine its Category", featureType.Name), "featureType");

            if (featureCategoryType == typeof(PluginFeature) || !typeof(PluginFeature).IsAssignableFrom(featureCategoryType))
                throw new ArgumentException(string.Format("Plugin Feature found [{0}], but its Base Type is not a valid Feature Category Type (Base Feature or not assignable)", featureType.Name), "featureType");

            var featureCategoryAttribute = (PluginFeatureCategoryAttribute)featureCategoryType.GetCustomAttributes(typeof(PluginFeatureCategoryAttribute), false).FirstOrDefault();

            if (featureCategoryAttribute == null)
                throw new ArgumentException(string.Format("Plugin Feature found [{0}], but its Base Type is not a valid Feature Category Type (no attribute)", featureType.Name), "featureType");

            return new PluginFeatureManifest()
            {
                PluginManifest = pluginManifest,
                Id = featureId,
                Name = featureName,
                Type = featureType,
                TypeName = featureType.FullName,
                CategoryType = featureCategoryType,
                CategoryTypeName = featureCategoryType.FullName
            };
        }
    }
}
