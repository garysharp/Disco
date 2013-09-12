using Disco.BI.Extensions;
using Disco.Data.Repository;
using Disco.Services.Logging;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.BI.DeviceBI.Migration
{
    public class LogMacAddressImporting : ScheduledTask
    {
        public override string TaskName { get { return "Migration: Logs to Device Mac Address Details"; } }

        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }

        #region Required Helpers
        private static string RequiredFilePath(DiscoDataContext dbContext)
        {
            if (dbContext.DiscoConfiguration.DataStoreLocation != null)
                return System.IO.Path.Combine(dbContext.DiscoConfiguration.DataStoreLocation, "_LogMacAddressImportingRequired.txt");
            else
                return null;
        }

        public static bool IsRequired(DiscoDataContext dbContext)
        {
            var requiredFilePath = RequiredFilePath(dbContext);

            if (requiredFilePath == null)
                return false;
            else
                return System.IO.File.Exists(requiredFilePath);
        }
        public static void SetRequired(DiscoDataContext dbContext)
        {
            var requiredFilePath = RequiredFilePath(dbContext);

            if (requiredFilePath != null)
            {
                System.IO.File.WriteAllText(requiredFilePath, "This file exists to indicate an Importing of Mac Address from the Disco Logs is required. It will automatically be deleted when the import completes.");
                System.IO.File.SetAttributes(requiredFilePath, System.IO.FileAttributes.Hidden);
            }
            // ELSE: Could never be required if no DataStoreLocation is set (no logs to process)
        }
        #endregion

        public override void InitalizeScheduledTask(DiscoDataContext dbContext)
        {
            if (IsRequired(dbContext))
            {
                // Schedule in 15mins
                var trigger = TriggerBuilder.Create()
                    .StartAt(DateTimeOffset.Now.AddMinutes(5));

                this.ScheduleTask(trigger);
            }
        }

        public static ScheduledTaskStatus ScheduleImmediately()
        {
            var existingTask = ScheduledTasks.GetTaskStatuses(typeof(LogMacAddressImporting)).Where(s => s.IsRunning).FirstOrDefault();
            if (existingTask != null)
                return existingTask;

            var instance = new LogMacAddressImporting();
            return instance.ScheduleTask();
        }

        protected override void ExecuteTask()
        {
            using (DiscoDataContext dbContext = new DiscoDataContext())
            {
                Status.UpdateStatus(0, "Importing MAC Addresses", "Querying Logs for Details");
                // Load Logs
                var logRetriever = new ReadLogContext()
                {
                    Module = DeviceBI.EnrolmentLog.Current.ModuleId,
                    EventTypes = new List<int>() { (int)DeviceBI.EnrolmentLog.EventTypeIds.SessionDeviceInfo }
                };
                var results = logRetriever.Query(dbContext);

                Status.UpdateStatus(50, string.Format("Passing {0} logs", results.Count));

                Dictionary<string, Tuple<string, string>> addresses = new Dictionary<string, Tuple<string, string>>();

                foreach (var result in results.OrderBy(r => r.Timestamp))
                    addresses[((string)result.Arguments[1]).ToLower()] = new Tuple<string, string>((string)result.Arguments[4], (string)result.Arguments[5]);

                Status.UpdateStatus(75, string.Format("Importing {0} details", addresses.Count));

                var devices = dbContext.Devices.Include("DeviceDetails").ToList();

                Tuple<string, string> addressResult;
                foreach (var device in devices)
                {
                    if (addresses.TryGetValue(device.SerialNumber.ToLower(), out addressResult))
                    {
                        if (!string.IsNullOrEmpty(addressResult.Item1))
                            device.DeviceDetails.LanMacAddress(device, addressResult.Item1);
                        if (!string.IsNullOrEmpty(addressResult.Item2))
                            device.DeviceDetails.WLanMacAddress(device, addressResult.Item2);
                    }
                }

                Status.UpdateStatus(90, "Saving to Database");

                dbContext.SaveChanges();

                // Finished - Remove Placeholder File
                var requiredFilePath = RequiredFilePath(dbContext);
                if (System.IO.File.Exists(requiredFilePath))
                    System.IO.File.Delete(requiredFilePath);
            }
        }


    }
}
