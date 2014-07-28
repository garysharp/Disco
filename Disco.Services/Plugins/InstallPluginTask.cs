using Disco.Data.Repository;
using Disco.Models.Services.Interop.DiscoServices;
using Disco.Services.Interop.DiscoServices;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;

namespace Disco.Services.Plugins
{
    public class InstallPluginTask : ScheduledTask
    {
        public override string TaskName { get { return "Installing Plugin"; } }

        protected override void ExecuteTask()
        {
            string packageUrlPath = (string)this.ExecutionContext.JobDetail.JobDataMap["PackageUrl"];
            string packageFilePath = (string)this.ExecutionContext.JobDetail.JobDataMap["PackageFilePath"];
            bool DeletePackageAfterInstall = (bool)this.ExecutionContext.JobDetail.JobDataMap["DeletePackageAfterInstall"];

            if (!Plugins.PluginsLoaded)
                throw new InvalidOperationException("Plugins have not been initialized");

            if (!string.IsNullOrEmpty(packageUrlPath))
            {
                this.Status.UpdateStatus(0, "Downloading Plugin Package", "Connecting...");

                if (File.Exists(packageFilePath))
                    File.Delete(packageFilePath);

                if (!Directory.Exists(Path.GetDirectoryName(packageFilePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(packageFilePath));

                // Need to Download the Package
                using (HttpClient httpClient = new HttpClient())
                {
                    using (var httpResponse = httpClient.GetAsync(packageUrlPath).Result)
                    {
                        httpResponse.EnsureSuccessStatusCode();

                        using (FileStream fsOut = new FileStream(packageFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            httpResponse.Content.ReadAsStreamAsync().Result.CopyTo(fsOut);
                        }
                    }
                }
            }

            this.Status.UpdateStatus(10, "Opening Plugin Package", Path.GetFileName(packageFilePath));

            using (var packageStream = File.OpenRead(packageFilePath))
            {
                using (ZipArchive packageArchive = new ZipArchive(packageStream, ZipArchiveMode.Read, false))
                {

                    ZipArchiveEntry packageManifestEntry = packageArchive.GetEntry("manifest.json");
                    if (packageManifestEntry == null)
                        throw new InvalidDataException("The plugin package does not contain the 'manifest.json' entry");

                    PluginManifest packageManifest;

                    using (Stream packageManifestStream = packageManifestEntry.Open())
                    {
                        packageManifest = PluginManifest.FromPluginManifestFile(packageManifestStream);
                    }

                    this.Status.UpdateStatus(20, string.Format("{0} [{1} v{2}] by {3}", packageManifest.Name, packageManifest.Id, packageManifest.Version.ToString(4), packageManifest.Author), "Initializing Install Environment");

                    PluginsLog.LogInstalling(packageManifest);
                    
                    lock (Plugins._PluginLock)
                    {
                        if (!Plugins.PluginsLoaded)
                            throw new InvalidOperationException("Plugins have not been initialized");

                        // Ensure not already installed
                        if (Plugins.GetPlugins().FirstOrDefault(p => p.Id == packageManifest.Id) != null)
                            throw new InvalidOperationException(string.Format("The '{0} [{1}]' Plugin is already installed, please uninstall any existing versions before trying again", packageManifest.Name, packageManifest.Id));

                        using (DiscoDataContext database = new DiscoDataContext())
                        {
                            string packagePath = Path.Combine(database.DiscoConfiguration.PluginsLocation, packageManifest.Id);

                            // Check for Compatibility
                            var libraryIncompatibility = PluginLibrary.LoadManifest(database).LoadIncompatibilityData();
                            PluginIncompatibility incompatibility;
                            if (!libraryIncompatibility.IsCompatible(packageManifest.Id, packageManifest.Version, out incompatibility))
                                throw new InvalidOperationException(string.Format("The plugin [{0} v{1}] is not compatible: {2}", packageManifest.Id, packageManifest.VersionFormatted, incompatibility.Reason));

                            // Force Delete of Existing Folder
                            if (Directory.Exists(packagePath))
                            {
                                this.Status.UpdateStatus(25, "Removing Existing Files");
                                try
                                {
                                    Directory.Delete(packagePath, true);
                                }
                                catch (UnauthorizedAccessException)
                                {
                                    throw new InvalidOperationException("Unable to delete existing plugin files, they may be locked by another process. Please restart Disco ICT and try installing the plugin again.");
                                }
                            }
                            Directory.CreateDirectory(packagePath);

                            
                            this.Status.UpdateStatus(30, "Extracting Files");

                            double extractFileInterval = (double)50 / packageArchive.Entries.Count;
                            int countExtractedFiles = 0;

                            // Extract Package Contents
                            foreach (var packageEntry in packageArchive.Entries)
                            {
                                this.Status.UpdateStatus(30 + (countExtractedFiles++ * extractFileInterval), string.Format("Extracting File: {0}", packageEntry.FullName));

                                // Determine Extraction Path
                                var packageEntryTarget = Path.Combine(packagePath, packageEntry.FullName);

                                // Create Sub Directories
                                Directory.CreateDirectory(Path.GetDirectoryName(packageEntryTarget));

                                using (var packageEntryStream = packageEntry.Open())
                                {
                                    using (var packageTargetStream = File.Open(packageEntryTarget, FileMode.Create, FileAccess.Write, FileShare.None))
                                    {
                                        packageEntryStream.CopyTo(packageTargetStream);
                                    }
                                }
                            }

                            // Reload Manifest
                            packageManifest = PluginManifest.FromPluginManifestFile(Path.Combine(packagePath, "manifest.json"));

                            // Install Plugin
                            this.Status.UpdateStatus(80, "Initial Package Configuration");
                            packageManifest.InstallPlugin(database, this.Status);

                            // Initialize Plugin
                            this.Status.UpdateStatus(98, "Initializing Plugin for Use");
                            packageManifest.InitializePlugin(database);

                            // Add Plugin Manifest to Host Environment
                            Plugins.AddPlugin(packageManifest);

                            PluginsLog.LogInstalled(packageManifest);
                            this.Status.SetFinishedUrl(string.Format("/Config/Plugins/{0}", System.Web.HttpUtility.UrlEncode(packageManifest.Id)));
                            this.Status.UpdateStatus(100, "Plugin Installation Completed");
                        }
                    }
                }
            }

            if (DeletePackageAfterInstall)
                File.Delete(packageFilePath);
        }

        public static ScheduledTaskStatus InstallLocalPlugin(string PackageFilePath, bool DeletePackageAfterInstall)
        {
            return InstallPlugin(null, PackageFilePath, DeletePackageAfterInstall);
        }
        public static ScheduledTaskStatus InstallPlugin(string PackageUrl, string PackageFilePath, bool DeletePackageAfterInstall)
        {
            if (ScheduledTasks.GetTaskStatuses(typeof(InstallPluginTask)).Where(s => s.IsRunning).Count() > 0)
                throw new InvalidOperationException("A plugin is already being Installed");
            if (ScheduledTasks.GetTaskStatuses(typeof(UpdatePluginTask)).Where(s => s.IsRunning).Count() > 0)
                throw new InvalidOperationException("A plugin is being Updated");
            if (ScheduledTasks.GetTaskStatuses(typeof(UninstallPluginTask)).Where(s => s.IsRunning).Count() > 0)
                throw new InvalidOperationException("A plugin is being Uninstalled");

            JobDataMap taskData = new JobDataMap() { { "PackageUrl", PackageUrl }, { "PackageFilePath", PackageFilePath }, { "DeletePackageAfterInstall", DeletePackageAfterInstall } };

            var instance = new InstallPluginTask();

            return instance.ScheduleTask(taskData);
        }
    }
}
