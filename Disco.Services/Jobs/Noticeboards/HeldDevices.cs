using Disco.Data.Repository;
using Disco.Data.Repository.Monitor;
using Disco.Models.Repository;
using Disco.Models.Services.Jobs.Noticeboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Disco.Services.Jobs.Noticeboards
{
    public static class HeldDevices
    {
        public const string Name = "HeldDevices";

        private readonly static List<string> MonitorJobProperties = new List<string>() {
            "DeviceSerialNumber",
            "UserId",
            "ExpectedClosedDate",
            "ClosedDate",                                                           
            "WaitingForUserAction",
            "DeviceHeld",
            "DeviceReadyForReturn",
            "DeviceReturnedDate"
        };
        private readonly static List<string> MonitorJobMetaNonWarrantyProperties = new List<string>(){
            "AccountingChargeRequiredDate",
            "AccountingChargeAddedDate",
            "AccountingChargePaidDate"
        };
        private readonly static List<string> MonitorDeviceProperties = new List<string>(){
            "Location",
            "DeviceProfileId",
            "DeviceDomainId",
            "AssignedUserId",
        };
        private readonly static List<string> MonitorDeviceProfileProperties = new List<string>(){
            "DefaultOrganisationAddress"
        };
        private readonly static List<string> MonitorUserProperties = new List<string>(){
            "DisplayName"
        };

        static HeldDevices()
        {
            // Subscribe to Repository Notifications
            RepositoryMonitor.StreamAfterCommit.Where(e =>
                (e.EntityType == typeof(Job) &&
                    (e.EventType == RepositoryMonitorEventType.Added ||
                    e.EventType == RepositoryMonitorEventType.Deleted ||
                    (e.EventType == RepositoryMonitorEventType.Modified && e.ModifiedProperties.Any(p => MonitorJobProperties.Contains(p))))
                    ) ||
                (e.EntityType == typeof(JobMetaNonWarranty) &&
                    (e.EventType == RepositoryMonitorEventType.Added ||
                    e.EventType == RepositoryMonitorEventType.Deleted ||
                    (e.EventType == RepositoryMonitorEventType.Modified && e.ModifiedProperties.Any(p => MonitorJobMetaNonWarrantyProperties.Contains(p))))
                    ) ||
                (e.EntityType == typeof(Device) &&
                    (e.EventType == RepositoryMonitorEventType.Modified && e.ModifiedProperties.Any(p => MonitorDeviceProperties.Contains(p)))
                    ) ||
                (e.EntityType == typeof(DeviceProfile) &&
                    (e.EventType == RepositoryMonitorEventType.Modified && e.ModifiedProperties.Any(p => MonitorDeviceProfileProperties.Contains(p)))
                    ) ||
                (e.EntityType == typeof(User) &&
                    (e.EventType == RepositoryMonitorEventType.Modified && e.ModifiedProperties.Any(p => MonitorUserProperties.Contains(p)))
                    )
                )
                .DelayBuffer(TimeSpan.FromMilliseconds(500))
                .Subscribe(RepositoryEvent);
        }

        private static void RepositoryEvent(IEnumerable<RepositoryMonitorEvent> e)
        {
            List<string> deviceSerialNumbers = new List<string>();
            List<string> userIds = new List<string>();

            using (DiscoDataContext Database = new DiscoDataContext())
            {
                foreach (var i in e)
                {
                    if (i.EntityType == typeof(Job))
                    {
                        if (i.EventType == RepositoryMonitorEventType.Modified &&
                            i.ModifiedProperties.Contains("DeviceSerialNumber"))
                        {
                            var p = i.GetPreviousPropertyValue<string>("DeviceSerialNumber");
                            if (p != null)
                                deviceSerialNumbers.Add(p);
                        }

                        var j = (Job)i.Entity;
                        if (j.DeviceSerialNumber != null)
                            deviceSerialNumbers.Add(j.DeviceSerialNumber);
                    }
                    else if (i.EntityType == typeof(JobMetaNonWarranty))
                    {
                        var jmnw = (JobMetaNonWarranty)i.Entity;

                        if (jmnw.Job != null)
                        {
                            if (jmnw.Job.DeviceSerialNumber != null)
                                deviceSerialNumbers.Add(jmnw.Job.DeviceSerialNumber);
                        }
                        else
                        {
                            var sn = Database.Jobs.Where(j => j.Id == jmnw.JobId).Select(j => j.DeviceSerialNumber).FirstOrDefault();
                            if (sn != null)
                                deviceSerialNumbers.Add(sn);
                        }
                    }
                    else if (i.EntityType == typeof(Device))
                    {
                        var d = (Device)i.Entity;
                        deviceSerialNumbers.Add(d.SerialNumber);

                        if (i.EventType == RepositoryMonitorEventType.Modified &&
                            i.ModifiedProperties.Contains("AssignedUserId"))
                        {
                            var p = i.GetPreviousPropertyValue<string>("AssignedUserId");
                            if (p != null)
                                userIds.Add(p);
                        }
                    }
                    else if (i.EntityType == typeof(DeviceProfile))
                    {
                        var dp = (DeviceProfile)i.Entity;

                        deviceSerialNumbers.AddRange(
                            Database.Jobs
                                .Where(j => !j.ClosedDate.HasValue && j.Device.DeviceProfileId == dp.Id)
                                .Select(j => j.DeviceSerialNumber)
                            );
                    }
                    else if (i.EntityType == typeof(User))
                    {
                        var u = (User)i.Entity;

                        deviceSerialNumbers.AddRange(
                            Database.Jobs
                                .Where(j => !j.ClosedDate.HasValue && j.Device.AssignedUserId == u.UserId)
                                .Select(j => j.DeviceSerialNumber)
                            );
                    }
                }

                deviceSerialNumbers = deviceSerialNumbers.Distinct().ToList();

                // Determine Held Devices for Users
                userIds.AddRange(
                    Database.Devices
                        .Where(d => d.AssignedUserId != null && deviceSerialNumbers.Contains(d.SerialNumber))
                        .Select(d => d.AssignedUserId)
                    );
                userIds = userIds.Distinct().ToList();


                // Notify Held Devices
                HeldDevices.BroadcastUpdates(Database, deviceSerialNumbers);

                // Notify Held Devices for Users
                HeldDevicesForUsers.BroadcastUpdates(Database, userIds);
            }
        }

        internal static void BroadcastUpdates(DiscoDataContext Database, List<string> DeviceSerialNumbers)
        {
            var jobs = Database.Jobs.Where(j => DeviceSerialNumbers.Contains(j.DeviceSerialNumber));

            var items = GetHeldDevices(jobs).ToDictionary(i => i.DeviceSerialNumber, StringComparer.OrdinalIgnoreCase);

            for (int skipAmount = 0; skipAmount < DeviceSerialNumbers.Count; skipAmount = skipAmount + 30)
            {
                var updates = DeviceSerialNumbers
                    .Skip(skipAmount).Take(30)
                    .ToDictionary(dsn => dsn, 
                    dsn => {
                        IHeldDeviceItem item;
                        items.TryGetValue(dsn, out item);
                        return item;
                    });

                NoticeboardUpdatesHub.HubContext.Clients
                    .Group(HeldDevices.Name)
                    .updateHeldDevice(updates);
            }
        }

        private static IEnumerable<IHeldDeviceItem> GetHeldDevices(IQueryable<Job> query)
        {
            var jobs = query
                .Where(j =>
                    !j.ClosedDate.HasValue &&
                    j.DeviceSerialNumber != null &&
                    ((j.DeviceHeld.HasValue && !j.DeviceReturnedDate.HasValue) || j.WaitingForUserAction.HasValue)
                    )
                .SelectHeldDeviceItems()
                .GroupBy(j => j.DeviceSerialNumber);

            foreach (var job in jobs.ToList())
            {
                if (job.Any(j => j.WaitingForUserAction))
                {
                    var item = job.Where(j => j.WaitingForUserAction).OrderBy(j => j.WaitingForUserActionSince).First();

                    yield return item;
                }
                else
                {
                    if (job.All(j => j.ReadyForReturn))
                    {
                        var item = job.OrderByDescending(j => j.ReadyForReturnSince).First();

                        yield return item;
                    }
                    else
                    {
                        var item = job.Where(j => !j.ReadyForReturn).OrderByDescending(j => j.EstimatedReturnTime).First();

                        yield return item;
                    }
                }
            }
        }

        public static IEnumerable<IHeldDeviceItem> GetHeldDevices(DiscoDataContext Database)
        {
            return GetHeldDevices(Database.Jobs);
        }
        public static IHeldDeviceItem GetHeldDevice(DiscoDataContext Database, string DeviceSerialNumber)
        {
            return GetHeldDevices(Database.Jobs.Where(j => j.DeviceSerialNumber == DeviceSerialNumber)).FirstOrDefault();
        }

        internal static IEnumerable<HeldDeviceItem> SelectHeldDeviceItems(this IQueryable<Job> jobs)
        {
            return HeldDeviceItem.FromJobs(jobs);
        }
    }
}
