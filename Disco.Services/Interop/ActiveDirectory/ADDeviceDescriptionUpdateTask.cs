using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.IO;
using System.Linq;

namespace Disco.Services.Interop.ActiveDirectory
{
    public class ADDeviceDescriptionUpdateTask : ScheduledTask
    {
        public override string TaskName { get { return "Active Directory Device Description Update"; } }

        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }

        #region Required Helpers
        private static string RequiredFilePath(DiscoDataContext Database)
        {
            if (Database.DiscoConfiguration.DataStoreLocation != null)
                return Path.Combine(Database.DiscoConfiguration.DataStoreLocation, "_ADDeviceDescriptionUpdateRequired.txt");
            else
                return null;
        }

        public static bool IsRequired(DiscoDataContext Database)
        {
            var requiredFilePath = RequiredFilePath(Database);

            if (requiredFilePath == null)
                return false;
            else
                return File.Exists(requiredFilePath);
        }
        public static void SetRequired(DiscoDataContext Database)
        {
            var requiredFilePath = RequiredFilePath(Database);

            if (requiredFilePath != null)
            {
                File.WriteAllText(requiredFilePath, "This file exists to indicate an update to AD Device Descriptions is required. It will automatically be deleted when the update completes.");
                File.SetAttributes(requiredFilePath, FileAttributes.Hidden);
            }
        }
        #endregion

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            if (IsRequired(Database))
            {
                // Schedule in 5mins
                var trigger = TriggerBuilder.Create()
                    .StartAt(DateTimeOffset.Now.AddMinutes(3));

                ScheduleTask(trigger);
            }
        }

        public static ScheduledTaskStatus ScheduleImmediately()
        {
            var existingTask = ScheduledTasks.GetTaskStatuses(typeof(ADDeviceDescriptionUpdateTask)).Where(s => s.IsRunning).FirstOrDefault();
            if (existingTask != null)
                return existingTask;

            var instance = new ADDeviceDescriptionUpdateTask();
            return instance.ScheduleTask();
        }

        protected override void ExecuteTask()
        {
            using (DiscoDataContext database = new DiscoDataContext())
            {
                Status.UpdateStatus(0, "Updating Active Directory Device Descriptions", "Reading Devices");

                // Devices
                var devices = database.Devices.Where(d => d.DeviceDomainId != null)
                    .ToList();

                int failedTotal = 0;
                int notFoundTotal = 0;
                int completedTotal = 0;

                // Refine valid devices
                devices = devices.Where(d => ActiveDirectory.IsValidDomainAccountId(d.DeviceDomainId)).ToList();

                foreach (var domainGroup in devices.GroupBy(d => d.ComputerDomainName).ToList())
                {
                    if (domainGroup.Key != null && ActiveDirectory.Context.TryGetDomainByNetBiosName(domainGroup.Key, out var domain))
                    {
                        var controller = domain.GetAvailableDomainController(RequireWritable: true);

                        foreach (var device in domainGroup)
                        {
                            completedTotal++;
                            if ((completedTotal % 10) == 0)
                            {
                                Status.UpdateStatus((100D / devices.Count) * completedTotal, $"Processing: {device.DeviceDomainId} ({device.SerialNumber})");
                            }
                            try
                            {
                                var adAccount = device.ActiveDirectoryAccount();
                                if (adAccount == null)
                                {
                                    notFoundTotal++;

                                    if (!device.DecommissionedDate.HasValue)
                                    {
                                        Status.LogWarning($"Unable to locate [{device.DeviceDomainId}] for commissioned device [{device.SerialNumber}] in the domain");
                                    }
                                }
                                else
                                {
                                    adAccount.SetDescription(controller, device);
                                }
                            }
                            catch (Exception ex)
                            {
                                failedTotal++;
                                Status.LogWarning($"Error when setting description of computer account [{device.DeviceDomainId}] for device [{device.SerialNumber}]: [{ex.GetType().Name}] {ex.Message}");
                            }
                        }
                    }
                }

                // Finished - Remove Placeholder File
                var requiredFilePath = RequiredFilePath(database);
                if (requiredFilePath != null && File.Exists(requiredFilePath))
                    File.Delete(requiredFilePath);

                Status.SetFinishedMessage($"Finished updating device descriptions for {devices.Count:N0}. {notFoundTotal:N0} were not found. {failedTotal:N0} failed.");
                Status.LogInformation(Status.FinishedMessage);
                Status.Finished();
            }
        }

    }
}
