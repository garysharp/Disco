using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disco.Services.Tasks;

namespace Disco.Services.Plugins
{
    public class UpdatePluginsAfterDiscoUpdateTask : ScheduledTask
    {
        private static object _startLock = new object();

        public override string TaskName { get { return "Updating Disco Plugins"; } }

        protected override void ExecuteTask()
        {
            this.Status.UpdateStatus(0, "Updating plugins after Disco update", "Starting, please wait...");

            // Wait for App to Load (6 Seconds)
            for (int i = 0; i < 10; i++)
            {
                this.Status.UpdateStatus(10 * i);                
                System.Threading.Thread.Sleep(600);
            }

            // Update Catalogue
            CommunityInterop.PluginLibraryUpdateTask.ExecuteTaskInternal(this.Status);

            // Update all Plugins
            UpdatePluginTask.UpdateOffline(this.Status);

            // Restart
            this.Status.Finished("Restarting Disco, please wait...", "/");
            Plugins.RestartApp(1500);
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
