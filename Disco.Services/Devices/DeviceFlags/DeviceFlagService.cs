using Disco.Data.Repository;
using Disco.Data.Repository.Monitor;
using Disco.Models.Repository;
using Disco.Services.Extensions;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reactive.Linq;

namespace Disco.Services.Devices.DeviceFlags
{
    public static class DeviceFlagService
    {
        private static Cache _cache;
        internal static Lazy<IObservable<RepositoryMonitorEvent>> DeviceFlagAssignmentRepositoryEvents;

        static DeviceFlagService()
        {
            // Statically defined (lazy) Assignment Repository Definition
            DeviceFlagAssignmentRepositoryEvents =
                new Lazy<IObservable<RepositoryMonitorEvent>>(() =>
                    RepositoryMonitor.StreamAfterCommit.Where(e =>
                        e.EntityType == typeof(DeviceFlagAssignment) &&
                        (e.EventType != RepositoryMonitorEventType.Modified ||
                        e.ModifiedProperties.Contains(nameof(DeviceFlagAssignment.RemovedDate)))
                        )
                    );
        }

        public static void Initialize(DiscoDataContext database)
        {
            _cache = new Cache(database);

            // Initialize Managed Groups (if configured)
            _cache.GetDeviceFlags().ForEach(uf =>
            {
                DeviceFlagDevicesManagedGroup.Initialize(uf);
                DeviceFlagDeviceAssignedUsersManagedGroup.Initialize(uf);
            });
        }

        public static List<DeviceFlag> GetDeviceFlags() { return _cache.GetDeviceFlags(); }
        public static DeviceFlag GetDeviceFlag(int deviceFlagId) { return _cache.GetDeviceFlag(deviceFlagId); }

        #region Device Flag Maintenance
        public static DeviceFlag CreateDeviceFlag(DiscoDataContext database, string name, string description)
        {
            // Verify
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The Device Flag Name is required", nameof(name));

            // Name Unique
            if (_cache.GetDeviceFlags().Any(f => f.Name.Equals(name, StringComparison.Ordinal)))
                throw new ArgumentException("Another Device Flag already exists with that name", nameof(name));

            // Clone to break reference
            var flag = new DeviceFlag()
            {
                Name = name,
                Description = description,
                Icon = RandomUnusedIcon(),
                IconColour = RandomUnusedThemeColour(),
            };

            database.DeviceFlags.Add(flag);
            database.SaveChanges();

            _cache.AddOrUpdate(flag);

            return flag;
        }
        public static DeviceFlag Update(DiscoDataContext database, DeviceFlag deviceFlag)
        {
            // Verify
            if (string.IsNullOrWhiteSpace(deviceFlag.Name))
                throw new ArgumentException("The Device Flag Name is required", nameof(deviceFlag));

            // Name Unique
            if (_cache.GetDeviceFlags().Any(f => f.Id != deviceFlag.Id && f.Name == deviceFlag.Name))
                throw new ArgumentException("Another Device Flag already exists with that name", nameof(deviceFlag));

            database.SaveChanges();

            _cache.AddOrUpdate(deviceFlag);
            DeviceFlagDevicesManagedGroup.Initialize(deviceFlag);
            DeviceFlagDeviceAssignedUsersManagedGroup.Initialize(deviceFlag);

            return deviceFlag;
        }
        public static void DeleteDeviceFlag(DiscoDataContext database, int deviceFlagId, IScheduledTaskStatus status)
        {
            var flag = database.DeviceFlags.Find(deviceFlagId);

            // Dispose of AD Managed Groups
            Interop.ActiveDirectory.ActiveDirectory.Context.ManagedGroups.Remove(DeviceFlagDeviceAssignedUsersManagedGroup.GetKey(flag));
            Interop.ActiveDirectory.ActiveDirectory.Context.ManagedGroups.Remove(DeviceFlagDevicesManagedGroup.GetKey(flag));

            // Delete Assignments
            status.UpdateStatus(0, $"Removing '{flag.Name}' [{flag.Id}] Device Flag", "Starting");
            var flagAssignments = database.DeviceFlagAssignments.Where(fa => fa.DeviceFlagId == flag.Id).ToList();
            if (flagAssignments.Count > 0)
            {
                status.UpdateStatus(20, "Removing flag from devices");
                flagAssignments.ForEach(flagAssignment => database.DeviceFlagAssignments.Remove(flagAssignment));
                database.SaveChanges();
            }

            // Delete Flag
            status.UpdateStatus(90, "Deleting Device Flag");
            database.DeviceFlags.Remove(flag);
            database.SaveChanges();

            // Remove from Cache
            _cache.Remove(deviceFlagId);

            status.Finished($"Successfully Deleted Device Flag: '{flag.Name}' [{flag.Id}]");
        }
        #endregion

        #region Bulk Assignment
        public static IEnumerable<DeviceFlagAssignment> BulkAssignAddDevices(DiscoDataContext database, DeviceFlag deviceFlag, User technician, string comments, List<Device> devices, IScheduledTaskStatus status)
        {
            if (devices.Count > 0)
            {
                double progressInterval;
                const int databaseChunkSize = 100;
                comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();

                var addDevices = devices.Where(d => !d.DeviceFlagAssignments.Any(a => a.DeviceFlagId == deviceFlag.Id && !a.RemovedDate.HasValue)).ToList();

                progressInterval = (double)100 / addDevices.Count;

                var addedDeviceAssignments = addDevices.Chunk(databaseChunkSize).SelectMany((chunk, chunkIndex) =>
                {
                    var chunkIndexOffset = databaseChunkSize * chunkIndex;

                    var chunkResults = chunk.Select((device, index) =>
                    {
                        status.UpdateStatus((chunkIndexOffset + index) * progressInterval, $"Assigning Flag: {device}");

                        return device.OnAddDeviceFlag(database, deviceFlag, technician, comments);
                    }).ToList();

                    // Save Chunk Items to Database
                    database.SaveChanges();

                    return chunkResults;
                }).Where(fa => fa != null).ToList();

                status.SetFinishedMessage($"{addDevices.Count} Devices/s Added; {(devices.Count - addDevices.Count)} Devices/s Skipped");

                return addedDeviceAssignments;
            }
            else
            {
                status.SetFinishedMessage("No changes found");
                return Enumerable.Empty<DeviceFlagAssignment>();
            }
        }

        public static IEnumerable<DeviceFlagAssignment> BulkAssignOverrideDevices(DiscoDataContext database, DeviceFlag deviceFlag, User technician, string comments, List<Device> devices, IScheduledTaskStatus status)
        {
            double progressInterval;
            const int databaseChunkSize = 100;
            comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();

            status.UpdateStatus(0, "Calculating assignment changes");

            var currentAssignments = database.DeviceFlagAssignments.Include(fa => fa.Device).Where(a => a.DeviceFlagId == deviceFlag.Id && !a.RemovedDate.HasValue).ToList();
            var removeAssignments = currentAssignments.Where(ca => !devices.Any(d => d.SerialNumber.Equals(ca.DeviceSerialNumber, StringComparison.OrdinalIgnoreCase))).ToList();
            var addAssignments = devices.Where(d => !currentAssignments.Any(ca => ca.DeviceSerialNumber.Equals(d.SerialNumber, StringComparison.OrdinalIgnoreCase))).ToList();

            if (removeAssignments.Count > 0 || addAssignments.Count > 0)
            {
                progressInterval = (double)100 / (removeAssignments.Count + addAssignments.Count);
                var removedDateTime = DateTime.Now;

                // Remove Assignments
                removeAssignments.Chunk(databaseChunkSize).SelectMany((chunk, chunkIndex) =>
                {
                    var chunkIndexOffset = (chunkIndex * databaseChunkSize) + removeAssignments.Count;

                    var chunkResults = chunk.Select((flagAssignment, index) =>
                    {
                        status.UpdateStatus((chunkIndexOffset + index) * progressInterval, $"Removing Flag: {flagAssignment.Device}");

                        flagAssignment.OnRemoveUnsafe(database, technician);

                        return flagAssignment;
                    }).ToList();

                    // Save Chunk Items to Database
                    database.SaveChanges();

                    return chunkResults;
                }).ToList();

                // Add Assignments
                var addedAssignments = addAssignments.Chunk(databaseChunkSize).SelectMany((chunk, chunkIndex) =>
                {
                    var chunkIndexOffset = (chunkIndex * databaseChunkSize) + removeAssignments.Count;

                    var chunkResults = chunk.Select((device, index) =>
                    {
                        status.UpdateStatus((chunkIndexOffset + index) * progressInterval, $"Assigning Flag: {device}");

                        return device.OnAddDeviceFlag(database, deviceFlag, technician, comments);
                    }).ToList();

                    // Save Chunk Items to Database
                    database.SaveChanges();

                    return chunkResults;
                }).ToList();

                status.SetFinishedMessage($"{addAssignments.Count} Devices/s Added; {removeAssignments.Count} Devices/s Removed; {(devices.Count - addAssignments.Count)} Devices/s Skipped");

                return addedAssignments;
            }
            else
            {
                status.SetFinishedMessage("No changes found");
                return Enumerable.Empty<DeviceFlagAssignment>();
            }
        }
        #endregion

        public static string RandomUnusedIcon()
        {
            return UIHelpers.RandomIcon(_cache.GetDeviceFlags().Select(f => f.Icon));
        }
        public static string RandomUnusedThemeColour()
        {
            return UIHelpers.RandomThemeColour(_cache.GetDeviceFlags().Select(f => f.IconColour));
        }
    }
}
