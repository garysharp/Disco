using Disco.Data.Repository;
using Disco.Models.Services.Interop.DiscoServices;
using Disco.Services.Interop.DiscoServices;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;

namespace Disco.Services.Plugins
{
    public class UpdatePluginTask : ScheduledTask
    {
        public override string TaskName { get { return "Updating Plugin/s"; } }

        protected override void ExecuteTask()
        {
            var pluginId = (string)ExecutionContext.JobDetail.JobDataMap["PluginId"];
            var packageFilePath = (string)ExecutionContext.JobDetail.JobDataMap["PackageFilePath"];
            var immediateRestart = (bool)ExecutionContext.JobDetail.JobDataMap["ImmediateRestart"];

            PluginLibraryManifestV2 libraryManifest;
            PluginLibraryIncompatibility libraryIncompatibility;
            string pluginPackagesLocation;

            if (!Plugins.PluginsLoaded)
                throw new InvalidOperationException("Plugins have not been initialized");

            List<Tuple<PluginManifest, string, PluginLibraryItemV2, PluginLibraryItemReleaseV2>> updatePlugins;

            using (DiscoDataContext database = new DiscoDataContext())
            {
                libraryManifest = PluginLibrary.LoadManifest(database);
                libraryIncompatibility = libraryManifest.LoadIncompatibilityData();
                pluginPackagesLocation = database.DiscoConfiguration.PluginPackagesLocation;
            }

            if (!string.IsNullOrEmpty(pluginId))
            {
                if (string.IsNullOrEmpty(packageFilePath))
                {
                    // Update Single from Catalogue
                    PluginManifest existingManifest = Plugins.GetPlugin(pluginId);
                    var libraryItem = libraryManifest.Plugins.FirstOrDefault(p => p.Id == existingManifest.Id);

                    if (libraryItem == null)
                        throw new InvalidOperationException("This item isn't in the plugin library manifest");

                    var libraryItemRelease = libraryItem.LatestCompatibleRelease(libraryIncompatibility);

                    if (Version.Parse(libraryItemRelease.Version) <= existingManifest.Version)
                        throw new InvalidOperationException("Only newer versions can be used to update a plugin");

                    updatePlugins = new List<Tuple<PluginManifest, string, PluginLibraryItemV2, PluginLibraryItemReleaseV2>>() {
                        Tuple.Create(existingManifest, (string)null, libraryItem, libraryItemRelease)
                    };
                }
                else
                {
                    // Update Single from Local
                    PluginManifest existingManifest = Plugins.GetPlugin(pluginId);
                    updatePlugins = new List<Tuple<PluginManifest, string, PluginLibraryItemV2, PluginLibraryItemReleaseV2>>() {
                        Tuple.Create(existingManifest, packageFilePath, (PluginLibraryItemV2)null, (PluginLibraryItemReleaseV2)null)
                    };
                }
            }
            else
            {
                // Update All
                updatePlugins = Plugins.GetPlugins()
                    .Join(
                        libraryManifest.Plugins,
                        manifest => manifest.Id,
                        libraryItem => libraryItem.Id,
                        (manifest, libraryItem) => Tuple.Create(manifest, (string)null, libraryItem, libraryItem.LatestCompatibleRelease(libraryIncompatibility)),
                        StringComparer.OrdinalIgnoreCase)
                    .Where(i => Version.Parse(i.Item4.Version) > i.Item1.Version)
                    .ToList();
            }

            if (updatePlugins == null || updatePlugins.Count == 0)
            {
                Status.Finished("No plugins to update...", "/Config/Plugins");
                return;
            }

            ExecuteTaskInternal(Status, pluginPackagesLocation, updatePlugins);

            Status.Finished("Restarting Disco ICT, please wait...", "/Config/Plugins");
            Plugins.RestartApp(immediateRestart ? TimeSpan.Zero : TimeSpan.FromSeconds(1));
        }

        public static List<PluginManifest> OfflineInstalledPlugins(DiscoDataContext Database)
        {
            string pluginsLocation = Database.DiscoConfiguration.PluginsLocation;
            string pluginsStorageLocation = Database.DiscoConfiguration.PluginStorageLocation;

            List<PluginManifest> installedPluginManifests = new List<PluginManifest>();

            DirectoryInfo pluginDirectoryRoot = new DirectoryInfo(pluginsLocation);
            if (pluginDirectoryRoot.Exists)
            {
                foreach (DirectoryInfo pluginDirectory in pluginDirectoryRoot.EnumerateDirectories())
                {
                    string pluginManifestFilename = Path.Combine(pluginDirectory.FullName, "manifest.json");
                    if (File.Exists(pluginManifestFilename))
                    {
                        PluginManifest pluginManifest = null;
                        try
                        {
                            pluginManifest = PluginManifest.FromPluginManifestFile(pluginManifestFilename);
                        }
                        catch (Exception) { }

                        if (pluginManifest != null)
                            installedPluginManifests.Add(pluginManifest);
                    }
                }
            }

            return installedPluginManifests;
        }

        internal static void UpdateOffline(ScheduledTaskStatus Status)
        {
            PluginLibraryManifestV2 libraryManifest = null;
            PluginLibraryIncompatibility libraryIncompatibility = null;
            List<PluginManifest> installedPluginManifests;
            string pluginPackagesLocation;
            List<Tuple<PluginManifest, string, PluginLibraryItemV2, PluginLibraryItemReleaseV2>> updatePlugins =
                new List<Tuple<PluginManifest, string, PluginLibraryItemV2, PluginLibraryItemReleaseV2>>();


            using (DiscoDataContext database = new DiscoDataContext())
            {
                pluginPackagesLocation = database.DiscoConfiguration.PluginPackagesLocation;
                installedPluginManifests = OfflineInstalledPlugins(database);

                if (installedPluginManifests.Count > 0){
                    libraryManifest = PluginLibrary.LoadManifest(database);
                    libraryIncompatibility = libraryManifest.LoadIncompatibilityData();
                }
            }

            if (libraryManifest != null && installedPluginManifests.Count > 0)
            {
                foreach (var pluginManifest in installedPluginManifests)
                {
                    // Check for Update
                    var libraryItem = libraryManifest.Plugins.FirstOrDefault(i => i.Id == pluginManifest.Id);

                    if (libraryItem != null)
                    {
                        var libraryItemRelease = libraryItem.LatestCompatibleRelease(libraryIncompatibility);

                        if (libraryItemRelease != null && Version.Parse(libraryItemRelease.Version) > pluginManifest.Version)
                        { // Update Available
                            updatePlugins.Add(Tuple.Create(pluginManifest, (string)null, libraryItem, libraryItemRelease));
                        }
                    }
                }
            }

            if (updatePlugins.Count > 0)
            {
                ExecuteTaskInternal(Status, pluginPackagesLocation, updatePlugins);
            }
        }

        internal static void ExecuteTaskInternal(ScheduledTaskStatus Status, string pluginPackagesLocation, List<Tuple<PluginManifest, string, PluginLibraryItemV2, PluginLibraryItemReleaseV2>> UpdatePlugins)
        {
            while (UpdatePlugins.Count > 0)
            {
                var updatePlugin = UpdatePlugins[0];
                var existingManifest = updatePlugin.Item1;
                var packageTempFilePath = updatePlugin.Item2;
                var libraryItem = updatePlugin.Item3;
                var libraryItemRelease = updatePlugin.Item4;
                UpdatePlugins.Remove(updatePlugin);

                var pluginId = existingManifest != null ? existingManifest.Id : libraryItemRelease.PluginId;
                var pluginName = existingManifest != null ? existingManifest.Name : libraryItem.Name;

                if (string.IsNullOrEmpty(packageTempFilePath))
                {
                    // Download Update
                    Status.UpdateStatus(0, string.Format("Downloading Plugin Package: {0}", pluginName), "Connecting...");
                    packageTempFilePath = Path.Combine(pluginPackagesLocation, string.Format("{0}.discoPlugin", pluginId));

                    if (File.Exists(packageTempFilePath))
                        File.Delete(packageTempFilePath);

                    if (!Directory.Exists(Path.GetDirectoryName(packageTempFilePath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(packageTempFilePath));

                    // Need to Download the Package
                    using (HttpClient httpClient = new HttpClient())
                    {
                        Status.UpdateStatus(0, "Downloading...");

                        using (var httpResponse = httpClient.GetAsync(libraryItemRelease.DownloadUrl).Result)
                        {
                            httpResponse.EnsureSuccessStatusCode();

                            using (FileStream fsOut = new FileStream(packageTempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                httpResponse.Content.ReadAsStreamAsync().Result.CopyTo(fsOut);
                            }
                        }
                    }
                }

                Status.UpdateStatus(10, "Opening Plugin Package", Path.GetFileName(packageTempFilePath));

                PluginManifest updateManifest;

                using (var packageStream = File.OpenRead(packageTempFilePath))
                {
                    using (ZipArchive packageArchive = new ZipArchive(packageStream, ZipArchiveMode.Read, false))
                    {

                        ZipArchiveEntry packageManifestEntry = packageArchive.GetEntry("manifest.json");
                        if (packageManifestEntry == null)
                            throw new InvalidDataException("The plugin package does not contain the 'manifest.json' entry");

                        using (Stream packageManifestStream = packageManifestEntry.Open())
                        {
                            updateManifest = PluginManifest.FromPluginManifestFile(packageManifestStream);
                        }
                    }
                }

                // Ensure not already installed
                if (existingManifest != null)
                    if (updateManifest.Version < existingManifest.Version)
                        throw new InvalidOperationException("Older versions cannot update existing plugins");

                Status.UpdateStatus(20, string.Format("{0} [{1} v{2}] by {3}", updateManifest.Name, updateManifest.Id, updateManifest.Version.ToString(4), updateManifest.Author), "Initializing Update Environment");

                using (DiscoDataContext database = new DiscoDataContext())
                {
                    // Check for Compatibility
                    var incompatibilityLibrary = PluginLibrary.LoadManifest(database).LoadIncompatibilityData();
                    PluginIncompatibility incompatibility;
                    if (!incompatibilityLibrary.IsCompatible(updateManifest.Id, updateManifest.Version, out incompatibility))
                        throw new InvalidOperationException(string.Format("The plugin [{0} v{1}] is not compatible: {2}", updateManifest.Id, updateManifest.VersionFormatted, incompatibility.Reason));

                    var updatePluginPath = Path.Combine(database.DiscoConfiguration.PluginsLocation, string.Format("{0}.discoPlugin", updateManifest.Id));
                    File.Move(packageTempFilePath, updatePluginPath);

                    if (existingManifest != null)
                    {
                        PluginsLog.LogBeforeUpdate(existingManifest, updateManifest);
                    }
                }
            }
        }

        private static ScheduledTaskStatus UpdateHelper(string pluginId, string packageFilePath, bool immediateRestart)
        {
            if (ScheduledTasks.GetTaskStatuses(typeof(UpdatePluginTask)).Where(s => s.IsRunning).Count() > 0)
                throw new InvalidOperationException("A plugin is already being Updated");
            if (ScheduledTasks.GetTaskStatuses(typeof(UninstallPluginTask)).Where(s => s.IsRunning).Count() > 0)
                throw new InvalidOperationException("A plugin is being Uninstalled");
            if (ScheduledTasks.GetTaskStatuses(typeof(InstallPluginTask)).Where(s => s.IsRunning).Count() > 0)
                throw new InvalidOperationException("A plugin is being Installed");

            JobDataMap taskData = new JobDataMap() {
                { "PluginId", pluginId },
                { "PackageFilePath", packageFilePath },
                { "ImmediateRestart", immediateRestart },
            };

            var instance = new UpdatePluginTask();

            return instance.ScheduleTask(taskData);
        }

        public static ScheduledTaskStatus UpdateLocalPlugin(string pluginId, string packageFilePath, bool immediateRestart = false)
        {
            return UpdateHelper(pluginId, packageFilePath, immediateRestart);
        }
        public static ScheduledTaskStatus UpdatePlugin(string pluginId, bool immediateRestart = false)
        {
            return UpdateHelper(pluginId, packageFilePath: null, immediateRestart);
        }
        public static ScheduledTaskStatus UpdateAllPlugins()
        {
            return UpdateHelper(pluginId: null, packageFilePath: null, immediateRestart: false);
        }
    }
}
