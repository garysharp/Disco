using Disco.Data.Repository;
using Disco.Models.Services.Interop.DiscoServices;
using Disco.Services.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Disco.Services.Interop.DiscoServices
{
    public static class PluginLibrary
    {
        private static string UpdateUrl()
        {
            return string.Concat(DiscoServiceHelpers.ServicesUrl, "API/Plugins/Library/V2");
        }

        public static string InitialManifestFilename()
        {
            return HttpContext.Current.Server.MapPath("~/ClientBin/DiscoServices.InitialPluginLibraryManifest.json");
        }

        public static string ManifestFilename(DiscoDataContext Database)
        {
            return Path.Combine(Database.DiscoConfiguration.PluginPackagesLocation, "LibraryManifest.json");
        }

        public static PluginLibraryManifestV2 LoadManifest(DiscoDataContext Database)
        {
            var manifestFile = ManifestFilename(Database);

            if (File.Exists(manifestFile))
            {
                return JsonConvert.DeserializeObject<PluginLibraryManifestV2>(File.ReadAllText(manifestFile));
            }
            else
            {
                // Use Initial Plugin Library Manifest
                manifestFile = InitialManifestFilename();

                if (File.Exists(manifestFile))
                    return JsonConvert.DeserializeObject<PluginLibraryManifestV2>(File.ReadAllText(manifestFile));

                throw new FileNotFoundException("No plugin library manifest file was found", manifestFile);
            }
        }

        public static PluginLibraryIncompatibility LoadIncompatibilityData(this PluginLibraryManifestV2 LibraryManifest)
        {
            var pluginAssembly = typeof(PluginLibrary).Assembly;
            Version hostVersion = pluginAssembly.GetName().Version;

            return new PluginLibraryIncompatibility()
            {
                IncompatiblePlugins = LibraryManifest.Plugins.SelectMany(p => p.Releases, (p, r) =>
                {
                    var rVersion = Version.Parse(r.Version);

                    if (r.Blocked)
                        return new PluginIncompatibility() { PluginId = r.PluginId, Version = rVersion, Reason = "This plugin release is blocked by Disco ICT Online Services" };

                    if (r.HostMinVersion != null && hostVersion < Version.Parse(r.HostMinVersion))
                        return new PluginIncompatibility() { PluginId = r.PluginId, Version = rVersion, Reason = $"This plugin requires v{r.HostMinVersion} or newer" };

                    if (r.HostMaxVersion != null && hostVersion > Version.Parse(r.HostMaxVersion))
                        return new PluginIncompatibility() { PluginId = r.PluginId, Version = rVersion, Reason = $"This plugin requires v{r.HostMaxVersion} or older" };

                    return null;
                }).Where(i => i != null).ToList()
            };
        }

        public static PluginLibraryManifestV2 UpdateManifest(DiscoDataContext Database, IScheduledTaskStatus Status)
        {
            Status.UpdateStatus(10, "Sending Request");

            PluginLibraryManifestV2 result;

            var discoVersion = UpdateQuery.CurrentDiscoVersionFormatted();
            var url = UpdateUrl();

            using (var httpClient = new HttpClient())
            {
                using (var formData = new FormUrlEncodedContent(new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>("DeploymentId", Database.DiscoConfiguration.DeploymentId),
                    new KeyValuePair<string, string>("DiscoVersion", discoVersion)
                }))
                {
                    var response = httpClient.PostAsync(url, formData).Result;

                    response.EnsureSuccessStatusCode();

                    Status.UpdateStatus(50, "Waiting for Response");

                    var resultJson = response.Content.ReadAsStringAsync().Result;

                    Status.UpdateStatus(90, "Processing Response");

                    result = JsonConvert.DeserializeObject<PluginLibraryManifestV2>(resultJson);
                }
            }

            var manifestJson = JsonConvert.SerializeObject(result, Formatting.Indented);

            var manifestFile = PluginLibrary.ManifestFilename(Database);

            if (!Directory.Exists(Path.GetDirectoryName(manifestFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(manifestFile));

            File.WriteAllText(manifestFile, manifestJson);

            return result;
        }

        public static PluginLibraryItemReleaseV2 LatestCompatibleRelease(this PluginLibraryItemV2 LibraryItem, PluginLibraryIncompatibility Incompatibility)
        {
            return LibraryItem.Releases.OrderByDescending(r => Version.Parse(r.Version)).FirstOrDefault(r => Incompatibility.IsCompatible(r));
        }

        public static bool IsCompatible(this PluginLibraryIncompatibility IncompatibilityLibrary, PluginLibraryItemReleaseV2 Release)
        {
            PluginIncompatibility incompatibility;

            return IsCompatible(IncompatibilityLibrary, Release, out incompatibility);
        }

        public static bool IsCompatible(this PluginLibraryIncompatibility IncompatibilityLibrary, PluginLibraryItemReleaseV2 Release, out PluginIncompatibility Incompatibility)
        {
            return IsCompatible(IncompatibilityLibrary, Release.PluginId, Version.Parse(Release.Version), out Incompatibility);
        }

        public static bool IsCompatible(this PluginLibraryIncompatibility IncompatibilityLibrary, string PluginId, Version Version)
        {
            PluginIncompatibility incompatibility;

            return IsCompatible(IncompatibilityLibrary, PluginId, Version, out incompatibility);
        }

        public static bool IsCompatible(this PluginLibraryIncompatibility IncompatibilityLibrary, string PluginId, Version Version, out PluginIncompatibility Incompatibility)
        {
            Incompatibility = IncompatibilityLibrary.IncompatiblePlugins.FirstOrDefault(i => i.PluginId.Equals(PluginId, StringComparison.OrdinalIgnoreCase) && i.Version == Version);

            return Incompatibility == null;
        }
    }
}
