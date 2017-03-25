using System;
using System.Linq;
using Disco.Data.Repository;
using Newtonsoft.Json;

namespace Disco.Services.Plugins
{
    public class PluginFeatureManifest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public bool PrimaryFeature { get; set; }

        [JsonProperty]
        private string CategoryTypeName { get; set; }

        [JsonIgnore]
        public PluginManifest PluginManifest { get; private set; }
        [JsonIgnore]
        internal Type Type { get; private set; }
        [JsonIgnore]
        public Type CategoryType { get; private set; }

        internal bool Initialize(DiscoDataContext Database, PluginManifest pluginManifest)
        {
            PluginManifest = pluginManifest;

            if (Type == null)
                Type = PluginManifest.PluginAssembly.GetType(TypeName, true, true);

            if (CategoryType == null)
                CategoryType = Type.GetType(CategoryTypeName, true, true);

            using (var instance = CreateInstance())
            {
                instance.Initialize(Database);
            }

            PluginsLog.LogInitializedPluginFeature(PluginManifest, this);

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
            if (typeof(CategoryType).IsAssignableFrom(Type))
            {
                var i = (CategoryType)Activator.CreateInstance(Type);
                i.Manifest = this;
                return i;
            }
            else
                throw new InvalidOperationException(string.Format("The feature [{0}] cannot be cast into type [{1}]", Type.Name, typeof(CategoryType).Name));
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
            var featurePrimary = featureAttribute.PrimaryFeature;

            // Determine Feature Category
            var featureCategoryType = featureType.BaseType;

            if (featureCategoryType == null)
                throw new ArgumentException(string.Format("Plugin Feature found [{0}], but has no Base Type to determine its Category", featureType.Name), "featureType");

            // Handle Generic-Type Features (Only use base-feature, not generic parameters)
            if (featureCategoryType.IsGenericType)
                featureCategoryType = featureCategoryType.GetGenericTypeDefinition();

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
                PrimaryFeature = featurePrimary,
                Type = featureType,
                TypeName = featureType.FullName,
                CategoryType = featureCategoryType,
                CategoryTypeName = featureCategoryType.FullName
            };
        }
    }
}
