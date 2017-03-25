using System;

namespace Disco.Services.Plugins
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PluginFeatureAttribute : Attribute
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool PrimaryFeature { get; set; }
    }
}
