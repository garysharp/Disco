using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.IO;
using System.Linq;

namespace Disco.Services.Plugins
{
    public class UninstallPluginTask : ScheduledTask
    {
        public override string TaskName { get { return "Uninstalling Plugin"; } }

        protected override void ExecuteTask()
        {
            var manifest = (PluginManifest)ExecutionContext.JobDetail.JobDataMap["PluginManifest"];
            var UninstallData = (bool)ExecutionContext.JobDetail.JobDataMap["UninstallData"];

            Status.UpdateStatus(25, string.Format("Uninstalling Plugin: {0} [{1}]", manifest.Name, manifest.Id), "Queuing plugin for uninstall");

            PluginsLog.LogUninstalling(manifest, UninstallData);

            string manifestFileLocation = Path.Combine(manifest.PluginLocation, "manifest.json");
            if (!File.Exists(manifestFileLocation))
                throw new FileNotFoundException("Plugin Manifest File Not Found", manifestFileLocation);

            using (DiscoDataContext database = new DiscoDataContext())
            {
                manifest.UninstallPlugin(database, UninstallData, Status);
            }

            string manifestUninstallFileLocation = Path.Combine(manifest.PluginLocation, "manifest.uninstall.json");

            if (File.Exists(manifestUninstallFileLocation))
                File.Delete(manifestUninstallFileLocation);

            File.Move(manifestFileLocation, manifestUninstallFileLocation);

            if (UninstallData && Directory.Exists(manifest.StorageLocation))
            {
                var manifestDataUninstallFileLocation = Path.Combine(manifest.StorageLocation, "manifest.uninstall.json");
                if (File.Exists(manifestDataUninstallFileLocation))
                    File.Delete(manifestDataUninstallFileLocation);

                File.Copy(manifestUninstallFileLocation, manifestDataUninstallFileLocation);
            }

            Status.Finished("Restarting Disco ICT, please wait...", "/Config/Plugins");
            Plugins.RestartApp(TimeSpan.FromSeconds(1));
        }

        public static ScheduledTaskStatus UninstallPlugin(PluginManifest Manifest, bool UninstallData)
        {
            if (ScheduledTasks.GetTaskStatuses(typeof(InstallPluginTask)).Where(s => s.IsRunning).Count() > 0)
                throw new InvalidOperationException("A plugin is already being Uninstalled");
            if (ScheduledTasks.GetTaskStatuses(typeof(UpdatePluginTask)).Where(s => s.IsRunning).Count() > 0)
                throw new InvalidOperationException("A plugin is being Updated");
            if (ScheduledTasks.GetTaskStatuses(typeof(InstallPluginTask)).Where(s => s.IsRunning).Count() > 0)
                throw new InvalidOperationException("A plugin is being Installed");

            var taskData = new JobDataMap() { { "PluginManifest", Manifest }, { "UninstallData", UninstallData } };

            var instance = new UninstallPluginTask();

            return instance.ScheduleTask(taskData);
        }
    }
}
