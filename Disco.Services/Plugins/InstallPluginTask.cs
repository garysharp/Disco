using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;

namespace Disco.Services.Plugins
{
    public class InstallPluginTask : ScheduledTask
    {
        public override string TaskName { get { return "Installing Plugin"; } }

        protected override void ExecuteTask()
        {
            string packageFilePath = (string)this.ExecutionContext.JobDetail.JobDataMap["PackageFilePath"];
            bool DeletePackageAfterInstall = (bool)this.ExecutionContext.JobDetail.JobDataMap["DeletePackageAfterInstall"];

            if (!Plugins.PluginsLoaded)
                throw new InvalidOperationException("Plugins have not been initialized");

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

                        using (DiscoDataContext dbContext = new DiscoDataContext())
                        {
                            string packagePath = Path.Combine(dbContext.DiscoConfiguration.PluginsLocation, packageManifest.Id);

                            // Force Delete of Existing Folder
                            if (Directory.Exists(packagePath))
                            {
                                this.Status.UpdateStatus(25, "Removing Existing Files");
                                Directory.Delete(packagePath, true);
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
                            packageManifest.InstallPlugin(dbContext, this.Status);

                            // Initialize Plugin
                            this.Status.UpdateStatus(98, "Initializing Plugin for Use");
                            packageManifest.InitializePlugin(dbContext);

                            // Add Plugin Manifest to Host Environment
                            Plugins.AddPlugin(packageManifest);

                            PluginsLog.LogInstalled(packageManifest);
                            this.Status.Finished("Plugin Installation Completed", string.Format("/Config/Plugins/{0}", System.Web.HttpUtility.UrlEncode(packageManifest.Id)));
                        }
                    }
                }
            }

            if (DeletePackageAfterInstall)
                File.Delete(packageFilePath);
        }

        public static ScheduledTaskStatus InstallPlugin(string PackageFilePath, bool DeletePackageAfterInstall)
        {
            if (ScheduledTasks.GetTaskStatuses(typeof(InstallPluginTask)).Where(s => s.IsRunning).Count() > 0)
                throw new InvalidOperationException("A plugin is already being Installed");

            JobDataMap taskData = new JobDataMap() { { "PackageFilePath", PackageFilePath }, { "DeletePackageAfterInstall", DeletePackageAfterInstall } };

            var instance = new InstallPluginTask();

            return instance.ScheduleTask(taskData);
        }
    }
}
