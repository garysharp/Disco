using System;
using System.Collections.Generic;

namespace Disco.Models.Services.Interop.DiscoServices
{
    public class PluginLibraryManifestV2
    {
        public DateTime ManifestDate { get; set; }

        public List<PluginLibraryItemV2> Plugins { get; set; }
    }

    public class PluginLibraryItemV2
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string InformationUrl { get; set; }

        public string PrimaryFeatureCategory { get; set; }

        public string Description { get; set; }

        public List<PluginLibraryItemReleaseV2> Releases { get; set; }
    }

    public class PluginLibraryItemReleaseV2
    {
        public string PluginId { get; set; }
        public string Version { get; set; }

        public string HostMinVersion { get; set; }
        public string HostMaxVersion { get; set; }

        public bool Blocked { get; set; }

        public string Description { get; set; }

        public string DownloadUrl { get; set; }
    }
}
