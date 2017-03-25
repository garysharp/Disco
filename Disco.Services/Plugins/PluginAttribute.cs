using System;

namespace Disco.Services.Plugins
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PluginAttribute : Attribute
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string Url { get; set; }
        public string HostVersionMin { get; set; }
        public string HostVersionMax { get; set; }
    }
}
