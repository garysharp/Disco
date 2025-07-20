using Disco.Data.Repository;
using Disco.Services.Logging;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Devices.Enrolment
{
    public class LogMacAddressImportingTask : ScheduledTask
    {
        public override string TaskName { get { return "Migration: Logs to Device Mac Address Details"; } }

        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }

        #region Required Helpers
        private static string RequiredFilePath(DiscoDataContext Database)
        {
            if (Database.DiscoConfiguration.DataStoreLocation != null)
                return System.IO.Path.Combine(Database.DiscoConfiguration.DataStoreLocation, "_LogMacAddressImportingRequired.txt");
            else
                return null;
        }

        public static bool IsRequired(DiscoDataContext Database)
        {
            var requiredFilePath = RequiredFilePath(Database);

            if (requiredFilePath == null)
                return false;
            else
                return System.IO.File.Exists(requiredFilePath);
        }
        public static void SetRequired(DiscoDataContext Database)
        {
            var requiredFilePath = RequiredFilePath(Database);

            if (requiredFilePath != null)
            {
                System.IO.File.WriteAllText(requiredFilePath, "This file exists to indicate an Importing of Mac Address from the Disco Logs is required. It will automatically be deleted when the import completes.");
                System.IO.File.SetAttributes(requiredFilePath, System.IO.FileAttributes.Hidden);
            }
            // ELSE: Could never be required if no DataStoreLocation is set (no logs to process)
        }
        #endregion

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            if (IsRequired(Database))
            {
                // Schedule in 15mins
                var trigger = TriggerBuilder.Create()
                    .StartAt(DateTimeOffset.Now.AddMinutes(5));

                ScheduleTask(trigger);
            }
        }

        public static ScheduledTaskStatus ScheduleImmediately()
        {
            var existingTask = ScheduledTasks.GetTaskStatuses(typeof(LogMacAddressImportingTask)).Where(s => s.IsRunning).FirstOrDefault();
            if (existingTask != null)
                return existingTask;

            var instance = new LogMacAddressImportingTask();
            return instance.ScheduleTask();
        }

        protected override void ExecuteTask()
        {
            using (DiscoDataContext database = new DiscoDataContext())
            {
                Status.UpdateStatus(0, "Importing MAC Addresses", "Querying Logs for Details");
                // Load Logs
                var logRetriever = new ReadLogContext()
                {
                    Module = EnrolmentLog.Current.ModuleId,
                    EventTypes = new List<int>() { (int)EnrolmentLog.EventTypeIds.SessionDeviceInfo }
                };
                var results = logRetriever.Query(database);

                Status.UpdateStatus(50, $"Passing {results.Count} logs");

                Dictionary<string, Tuple<string, string>> addresses = new Dictionary<string, Tuple<string, string>>();

                foreach (var result in results.OrderBy(r => r.Timestamp))
                    addresses[((string)result.Arguments[1]).ToLower()] = new Tuple<string, string>((string)result.Arguments[4], (string)result.Arguments[5]);

                Status.UpdateStatus(75, $"Importing {addresses.Count} details");

                var devices = database.Devices.Include("DeviceDetails").ToList();

                foreach (var device in devices)
                {
                    if (addresses.TryGetValue(device.SerialNumber.ToLower(), out var addressResult))
                    {
                        if (!string.IsNullOrEmpty(addressResult.Item1))
                            device.DeviceDetails.LanMacAddress(device, addressResult.Item1);
                        if (!string.IsNullOrEmpty(addressResult.Item2))
                            device.DeviceDetails.WLanMacAddress(device, addressResult.Item2);
                    }
                }

                Status.UpdateStatus(90, "Saving to Database");

                database.SaveChanges();

                // Finished - Remove Placeholder File
                var requiredFilePath = RequiredFilePath(database);
                if (System.IO.File.Exists(requiredFilePath))
                    System.IO.File.Delete(requiredFilePath);
            }
        }


    }
}
