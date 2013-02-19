using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Disco.Data.Repository;
using Disco.Models.BI.Interop.Community;
using Disco.Services.Tasks;
using Quartz;

namespace Disco.Services.Plugins
{
    public class UpdatePluginTask : ScheduledTask
    {
        public override string TaskName { get { return "Updating Plugin/s"; } }

        protected override void ExecuteTask()
        {
            string pluginId = (string)this.ExecutionContext.JobDetail.JobDataMap["PluginId"];
            string packageFilePath = (string)this.ExecutionContext.JobDetail.JobDataMap["PackageFilePath"];

            PluginLibraryUpdateResponse catalogue;
            string pluginPackagesLocation;

            if (!Plugins.PluginsLoaded)
                throw new InvalidOperationException("Plugins have not been initialized");

            List<Tuple<PluginManifest, string, PluginLibraryItem>> updatePlugins;

            using (DiscoDataContext dbContext = new DiscoDataContext())
            {
                catalogue = Plugins.LoadCatalogue(dbContext);
                pluginPackagesLocation = dbContext.DiscoConfiguration.PluginPackagesLocation;
            }

            if (!string.IsNullOrEmpty(pluginId))
            {
                if (string.IsNullOrEmpty(packageFilePath))
                {
                    // Update Single from Catalogue
                    PluginManifest existingManifest = Plugins.GetPlugin(pluginId);
                    var catalogueItem = catalogue.Plugins.FirstOrDefault(p => p.Id == existingManifest.Id);

                    if (catalogueItem == null)
                        throw new InvalidOperationException("No updates are available for this Plugin");
                    if (Version.Parse(catalogueItem.LatestVersion) <= existingManifest.Version)
                        throw new InvalidOperationException("Only newer versions can be used to update a plugin");

                    updatePlugins = new List<Tuple<PluginManifest, string, PluginLibraryItem>>() {
                        new Tuple<PluginManifest,string,PluginLibraryItem>(existingManifest, null, catalogueItem)
                    };
                }
                else
                {
                    // Update Single from Local
                    PluginManifest existingManifest = Plugins.GetPlugin(pluginId);
                    updatePlugins = new List<Tuple<PluginManifest, string, PluginLibraryItem>>() {
                        new Tuple<PluginManifest,string,PluginLibraryItem>(existingManifest, packageFilePath, null)
                    };
                }
            }
            else
            {
                // Update All
                updatePlugins = Plugins.GetPlugins().Join((IEnumerable<PluginLibraryItem>)catalogue.Plugins, manifest => manifest.Id, update => update.Id, (manifest, update) => new Tuple<PluginManifest, string, PluginLibraryItem>(manifest, null, update)).Where(i => Version.Parse(i.Item3.LatestVersion) > i.Item1.Version).ToList();
            }

            if (updatePlugins == null || updatePlugins.Count == 0)
            {
                this.Status.Finished("No plugins to update...", "/Config/Plugins");
                return;
            }

            ExecuteTaskInternal(this.Status, pluginPackagesLocation, updatePlugins);

            this.Status.Finished("Restarting Disco, please wait...", "/Config/Plugins");
            Plugins.RestartApp(1500);
        }

        internal static void UpdateOffline(ScheduledTaskStatus Status)
        {
            string pluginsLocation;
            string pluginPackagesLocation;
            string pluginsStorageLocation;
            PluginLibraryUpdateResponse pluginCatalogue;
            List<Tuple<PluginManifest, string, PluginLibraryItem>> UpdatePlugins = new List<Tuple<PluginManifest, string, PluginLibraryItem>>();

            using (DiscoDataContext dbContext = new DiscoDataContext())
            {
                pluginCatalogue = Plugins.LoadCatalogue(dbContext);
                pluginsLocation = dbContext.DiscoConfiguration.PluginsLocation;
                pluginPackagesLocation = dbContext.DiscoConfiguration.PluginPackagesLocation;
                pluginsStorageLocation = dbContext.DiscoConfiguration.PluginStorageLocation;
            }

            DirectoryInfo pluginDirectoryRoot = new DirectoryInfo(pluginsLocation);
            if (pluginDirectoryRoot.Exists)
            {
                MirgrateV1Plugins(pluginsLocation, pluginsStorageLocation);

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
                        {
                            // Check for Update
                            var catalogueItem = pluginCatalogue.Plugins.FirstOrDefault(i => i.Id == pluginManifest.Id && Version.Parse(i.LatestVersion) > pluginManifest.Version);

                            if (catalogueItem != null)
                            { // Update Available
                                UpdatePlugins.Add(new Tuple<PluginManifest, string, PluginLibraryItem>(pluginManifest, null, catalogueItem));
                            }
                        }
                    }
                }
            }

            if (UpdatePlugins.Count > 0)
            {
                ExecuteTaskInternal(Status, pluginPackagesLocation, UpdatePlugins);
            }
        }

        internal static void MirgrateV1Plugins(string pluginsLocation, string pluginsStorageLocation)
        {
            var migrationPackage = Path.Combine(HttpRuntime.BinDirectory, "Disco1.1-1.2PluginMigration.zip");

            if (File.Exists(migrationPackage))
            {
                // eduSTAR.net
                var eduSTARPluginPath = Path.Combine(pluginsLocation, "EduSTARnetCertificateProvider");
                if (Directory.Exists(eduSTARPluginPath))
                {
                    var eduSTARPluginAssemblyPath = Path.Combine(eduSTARPluginPath, "EduSTARnetCertificateProvider.dll");
                    if (File.Exists(eduSTARPluginAssemblyPath))
                    {
                        // Delete Old Plugin
                        Directory.Delete(eduSTARPluginPath, true);

                        // Add New Plugin
                        eduSTARPluginPath = Path.Combine(pluginsLocation, "eduSTARnet");
                        using (var migrationZipPackageStream = new FileStream(migrationPackage, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            using (ZipArchive migrationZipPackage = new ZipArchive(migrationZipPackageStream))
                            {
                                var pluginZipPackage = migrationZipPackage.Entries.Where(e => e.Name.Equals("eduSTARnet.discoPlugin", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                                if (pluginZipPackage != null)
                                {
                                    using (var pluginPackageStream = pluginZipPackage.Open())
                                    {
                                        using (ZipArchive pluginPackageArchive = new ZipArchive(pluginPackageStream))
                                        {
                                            foreach (var entry in pluginPackageArchive.Entries)
                                            {
                                                var entryPath = Path.Combine(eduSTARPluginPath, entry.FullName);
                                                Directory.CreateDirectory(Path.GetDirectoryName(entryPath));
                                                using (var entryOutput = new FileStream(entryPath, FileMode.Create, FileAccess.Write, FileShare.None))
                                                {
                                                    using (var entryStream = entry.Open())
                                                    {
                                                        entryStream.CopyTo(entryOutput);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // LWT
                var LWTPluginPath = Path.Combine(pluginsLocation, "LWTWarrantyProvider");
                if (Directory.Exists(LWTPluginPath))
                {
                    var LWTPluginAssemblyPath = Path.Combine(LWTPluginPath, "LWTWarrantyProvider.dll");
                    if (File.Exists(LWTPluginAssemblyPath))
                    {
                        // Delete Old Plugin
                        Directory.Delete(LWTPluginPath, true);
                        // Delete Plugin Storage
                        var LWTPluginStoragePath = Path.Combine(pluginsStorageLocation, "LWTWarrantyProvider");
                        if (Directory.Exists(LWTPluginStoragePath))
                            Directory.Delete(LWTPluginStoragePath, true);

                        // Add New Plugin
                        LWTPluginPath = Path.Combine(pluginsLocation, "LWTPlugin");
                        using (var migrationZipPackageStream = new FileStream(migrationPackage, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            using (ZipArchive migrationZipPackage = new ZipArchive(migrationZipPackageStream))
                            {
                                var pluginZipPackage = migrationZipPackage.Entries.Where(e => e.Name.Equals("LWTPlugin.discoPlugin", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                                if (pluginZipPackage != null)
                                {
                                    using (var pluginPackageStream = pluginZipPackage.Open())
                                    {
                                        using (ZipArchive pluginPackageArchive = new ZipArchive(pluginPackageStream))
                                        {
                                            foreach (var entry in pluginPackageArchive.Entries)
                                            {
                                                var entryPath = Path.Combine(LWTPluginPath, entry.FullName);
                                                Directory.CreateDirectory(Path.GetDirectoryName(entryPath));
                                                using (var entryOutput = new FileStream(entryPath, FileMode.Create, FileAccess.Write, FileShare.None))
                                                {
                                                    using (var entryStream = entry.Open())
                                                    {
                                                        entryStream.CopyTo(entryOutput);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }

        internal static void ExecuteTaskInternal(ScheduledTaskStatus Status, string pluginPackagesLocation, List<Tuple<PluginManifest, string, PluginLibraryItem>> UpdatePlugins)
        {
            while (UpdatePlugins.Count > 0)
            {
                var updatePlugin = UpdatePlugins[0];
                var existingManifest = updatePlugin.Item1;
                var packageTempFilePath = updatePlugin.Item2;
                var catalogueItem = updatePlugin.Item3;
                UpdatePlugins.Remove(updatePlugin);

                var pluginId = existingManifest != null ? existingManifest.Id : catalogueItem.Id;
                var pluginName = existingManifest != null ? existingManifest.Name : catalogueItem.Name;

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
                    WebClient downloader = new WebClient();
                    DateTime progressExpires = DateTime.Now;
                    downloader.DownloadProgressChanged += (sender, e) =>
                    {
                        Console.WriteLine(e.ProgressPercentage);
                        if (progressExpires <= DateTime.Now)
                        {
                            Status.UpdateStatus(e.ProgressPercentage, string.Format("{0} of {1} KB downloaded", e.BytesReceived / 1024, e.TotalBytesToReceive / 1024));
                            // Throttle Updates for SignalR
                            progressExpires = DateTime.Now.AddMilliseconds(250);
                        }
                    };
                    downloader.DownloadFileTaskAsync(new Uri(catalogueItem.LatestDownloadUrl), packageTempFilePath).Wait();
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
                    if (updateManifest.Version <= existingManifest.Version)
                        throw new InvalidOperationException("Only newer versions can be used to update a plugin");

                Status.UpdateStatus(20, string.Format("{0} [{1} v{2}] by {3}", updateManifest.Name, updateManifest.Id, updateManifest.Version.ToString(4), updateManifest.Author), "Initializing Update Environment");

                using (DiscoDataContext dbContext = new DiscoDataContext())
                {
                    // Check for Compatibility
                    var compatibilityData = Plugins.LoadCompatibilityData(dbContext);
                    var pluginCompatibility = compatibilityData.Plugins.FirstOrDefault(i => i.Id.Equals(updateManifest.Id, StringComparison.InvariantCultureIgnoreCase) && updateManifest.Version == Version.Parse(i.Version));
                    if (pluginCompatibility != null && !pluginCompatibility.Compatible)
                        throw new InvalidOperationException(string.Format("The plugin [{0} v{1}] is not compatible: {2}", updateManifest.Id, updateManifest.VersionFormatted, pluginCompatibility.Reason));

                    var updatePluginPath = Path.Combine(dbContext.DiscoConfiguration.PluginsLocation, string.Format("{0}.discoPlugin", updateManifest.Id));
                    File.Move(packageTempFilePath, updatePluginPath);

                    if (existingManifest != null)
                    {
                        PluginsLog.LogBeforeUpdate(existingManifest, updateManifest);
                    }
                }
            }
        }

        private static ScheduledTaskStatus UpdateHelper(string PluginId = null, string PackageFilePath = null)
        {
            if (ScheduledTasks.GetTaskStatuses(typeof(UpdatePluginTask)).Where(s => s.IsRunning).Count() > 0)
                throw new InvalidOperationException("A plugin is already being Updated");
            if (ScheduledTasks.GetTaskStatuses(typeof(UninstallPluginTask)).Where(s => s.IsRunning).Count() > 0)
                throw new InvalidOperationException("A plugin is being Uninstalled");
            if (ScheduledTasks.GetTaskStatuses(typeof(InstallPluginTask)).Where(s => s.IsRunning).Count() > 0)
                throw new InvalidOperationException("A plugin is being Installed");

            JobDataMap taskData = new JobDataMap() { { "PluginId", PluginId }, { "PackageFilePath", PackageFilePath } };

            var instance = new UpdatePluginTask();

            return instance.ScheduleTask(taskData);
        }

        public static ScheduledTaskStatus UpdateLocalPlugin(string PluginId, string PackageFilePath)
        {
            return UpdateHelper(PluginId, PackageFilePath);
        }
        public static ScheduledTaskStatus UpdatePlugin(string PluginId)
        {
            return UpdateHelper(PluginId);
        }
        public static ScheduledTaskStatus UpdateAllPlugins()
        {
            return UpdateHelper();
        }
    }
}
