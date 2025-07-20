using System;

namespace Disco.Models.Services.Interop.DiscoServices
{
    public class PluginIncompatibility
    {
        public string PluginId { get; set; }
        public Version Version { get; set; }

        public string Reason { get; set; }
    }
}
