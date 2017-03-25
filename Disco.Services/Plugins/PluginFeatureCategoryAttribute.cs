using System;

namespace Disco.Services.Plugins
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PluginFeatureCategoryAttribute : Attribute
    {
        public string DisplayName { get; set; }
    }
}
