using Disco.Data.Repository;
using Disco.Services.Interop.DiscoServices;
using Disco.Services.Tasks;
using System;
using System.Linq;

namespace Disco.Services.Plugins
{
    public class UpdatePluginsAfterDiscoUpdateTask : ScheduledTask
    {
        private static object _startLock = new object();

        public override string TaskName { get { return "Updating Disco ICT Plugins"; } }

        protected override void ExecuteTask()
        {
            Status.UpdateStatus(0, "Updating plugins after Disco ICT update", "Starting, please wait...");

            // Wait for App to Load (3 Seconds)
            for (int i = 0; i < 3; i++)
            {
                Status.UpdateStatus(33 * i);
                System.Threading.Thread.Sleep(1000);
            }

            // Update Catalogue
            using (DiscoDataContext database = new DiscoDataContext())
            {
                PluginLibrary.UpdateManifest(database, Status);
            }

            // Update all Plugins
            UpdatePluginTask.UpdateOffline(Status);

            // Restart
            Status.Finished("Restarting Disco ICT, please wait...", "/");
            Plugins.RestartApp(2500);
        }

        public static ScheduledTaskStatus UpdateDiscoPlugins(bool ReturnExistingStatusIfRunning)
        {
            if (ScheduledTasks.GetTaskStatuses(typeof(UpdatePluginTask)).Where(s => s.IsRunning).Count() > 0)
                throw new InvalidOperationException("A plugin is already being Updated");
            if (ScheduledTasks.GetTaskStatuses(typeof(UninstallPluginTask)).Where(s => s.IsRunning).Count() > 0)
                throw new InvalidOperationException("A plugin is being Uninstalled");
            if (ScheduledTasks.GetTaskStatuses(typeof(InstallPluginTask)).Where(s => s.IsRunning).Count() > 0)
                throw new InvalidOperationException("A plugin is being Installed");

            var existingTask = ScheduledTasks.GetTaskStatuses(typeof(UpdatePluginsAfterDiscoUpdateTask)).Where(s => s.IsRunning).FirstOrDefault();
            if (existingTask != null)
            {
                if (ReturnExistingStatusIfRunning)
                    return existingTask;
                else
                    throw new InvalidOperationException("Plugins are already being updated");
            }

            lock (_startLock)
            {
                existingTask = ScheduledTasks.GetTaskStatuses(typeof(UpdatePluginsAfterDiscoUpdateTask)).Where(s => s.IsRunning).FirstOrDefault();
                if (existingTask != null)
                {
                    if (ReturnExistingStatusIfRunning)
                        return existingTask;
                    else
                        throw new InvalidOperationException("Plugins are already being updated");
                }

                var instance = new UpdatePluginsAfterDiscoUpdateTask();
                var status = instance.ScheduleTask();

                // Give the scheduler a chance to start the task
                System.Threading.Thread.Sleep(150);

                return status;
            }
        }
    }
}
