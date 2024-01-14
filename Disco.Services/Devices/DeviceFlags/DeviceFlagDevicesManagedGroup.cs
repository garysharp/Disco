using Disco.Data.Repository;
using Disco.Data.Repository.Monitor;
using Disco.Models.Repository;
using Disco.Models.Services.Interop.ActiveDirectory;
using Disco.Services.Interop.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Disco.Services.Devices.DeviceFlags
{
    public class DeviceFlagDevicesManagedGroup : ADManagedGroup
    {
        private const string KeyFormat = "DeviceFlag_{0}_Devices";
        private const string DescriptionFormat = "Devices associated with the {0} Flag will be added to this Active Directory group.";
        private const string CategoryDescriptionFormat = "Assigned Devices Linked Group";
        private const string GroupDescriptionFormat = "{0} [Device Flag Devices]";

        private IDisposable repositorySubscription;
        private int deviceFlagId;
        private string deviceFlagName;

        public override string Description { get { return string.Format(DescriptionFormat, deviceFlagName); } }
        public override string CategoryDescription { get { return CategoryDescriptionFormat; } }
        public override string GroupDescription { get { return string.Format(GroupDescriptionFormat, deviceFlagName); } }
        public override bool IncludeFilterBeginDate { get { return true; } }

        private DeviceFlagDevicesManagedGroup(string Key, ADManagedGroupConfiguration Configuration, DeviceFlag DeviceFlag)
            : base(Key, Configuration)
        {
            deviceFlagId = DeviceFlag.Id;
            deviceFlagName = DeviceFlag.Name;
        }

        public override void Initialize()
        {
            // Subscribe to changes
            repositorySubscription = DeviceFlagService.DeviceFlagAssignmentRepositoryEvents.Value
                .Where(e =>
                    ((DeviceFlagAssignment)e.Entity).DeviceFlagId == deviceFlagId)
                .Subscribe(ProcessRepositoryEvent);
        }

        public static string GetKey(DeviceFlag deviceFlag)
        {
            return string.Format(KeyFormat, deviceFlag.Id);
        }
        public static string GetDescription(DeviceFlag deviceFlag)
        {
            return string.Format(DescriptionFormat, deviceFlag.Name);
        }
        public static string GetCategoryDescription(DeviceFlag deviceFlag)
        {
            return CategoryDescriptionFormat;
        }

        public static bool TryGetManagedGroup(DeviceFlag deviceFlag, out DeviceFlagDevicesManagedGroup managedGroup)
        {
            return ActiveDirectory.Context.ManagedGroups.TryGetValue(GetKey(deviceFlag), out managedGroup);
        }

        public static DeviceFlagDevicesManagedGroup Initialize(DeviceFlag deviceFlag)
        {
            if (deviceFlag.Id > 0)
            {
                var key = GetKey(deviceFlag);

                if (!string.IsNullOrEmpty(deviceFlag.DevicesLinkedGroup))
                {
                    var config = ConfigurationFromJson(deviceFlag.DevicesLinkedGroup);

                    if (config != null && !string.IsNullOrWhiteSpace(config.GroupId))
                    {
                        var group = new DeviceFlagDevicesManagedGroup(
                            key,
                            config,
                            deviceFlag);

                        // Add to AD Context
                        ActiveDirectory.Context.ManagedGroups.AddOrUpdate(group);

                        return group;
                    }
                }

                // Remove from AD Context
                ActiveDirectory.Context.ManagedGroups.Remove(key);
            }

            return null;
        }

        public override IEnumerable<string> DetermineMembers(DiscoDataContext Database)
        {
            var query = Database.DeviceFlagAssignments
                    .Where(a => a.DeviceFlagId == deviceFlagId && !a.RemovedDate.HasValue && a.Device.DeviceDomainId != null);

            if (Configuration.FilterBeginDate.HasValue)
                query = query.Where(a => a.AddedDate >= Configuration.FilterBeginDate);

            return query
                .Select(a => a.Device.DeviceDomainId)
                .ToList()
                .Where(ActiveDirectory.IsValidDomainAccountId)
                .Select(id => id + "$");
        }

        private void ProcessRepositoryEvent(RepositoryMonitorEvent Event)
        {
            var assignment = (DeviceFlagAssignment)Event.Entity;

            var domainId = assignment.Device?.DeviceDomainId;
            
            if (!ActiveDirectory.IsValidDomainAccountId(domainId))
                return;
            domainId += "$";

            switch (Event.EventType)
            {
                case RepositoryMonitorEventType.Added:
                    if (Configuration.FilterBeginDate.HasValue)
                    {
                        if (!assignment.RemovedDate.HasValue && assignment.AddedDate >= Configuration.FilterBeginDate)
                        {
                            AddMember(domainId);
                        }
                    }
                    else
                    {
                        if (!assignment.RemovedDate.HasValue)
                        {
                            AddMember(domainId);
                        }
                    }
                    break;
                case RepositoryMonitorEventType.Modified:
                    if (Configuration.FilterBeginDate.HasValue)
                    {
                        if (assignment.AddedDate >= Configuration.FilterBeginDate)
                        {
                            if (assignment.RemovedDate.HasValue)
                            {
                                RemoveMember(domainId);
                            }
                            else
                            {
                                AddMember(domainId);
                            }
                        }
                    }
                    else
                    {
                        if (assignment.RemovedDate.HasValue)
                        {
                            RemoveMember(domainId);
                        }
                        else
                        {
                            AddMember(domainId);
                        }
                    }
                    break;
                case RepositoryMonitorEventType.Deleted:
                    // Remove the device if no other (non-removed) assignments exist.
                    var serialNumber = assignment.DeviceSerialNumber;
                    RemoveMember(domainId, (database) =>
                    {
                        if (Configuration.FilterBeginDate.HasValue)
                        {
                            if (database.DeviceFlagAssignments.Any(a => a.DeviceFlagId == deviceFlagId && a.DeviceSerialNumber == serialNumber && !a.RemovedDate.HasValue && a.AddedDate >= Configuration.FilterBeginDate))
                            {
                                return null;
                            }
                            else
                            {
                                return new string[] { domainId };
                            }
                        }
                        else
                        {
                            if (database.DeviceFlagAssignments.Any(a => a.DeviceFlagId == deviceFlagId && a.DeviceSerialNumber == serialNumber && !a.RemovedDate.HasValue))
                            {
                                return null;
                            }
                            else
                            {
                                return new string[] { domainId };
                            }
                        }
                    });
                    break;
            }
        }

        public override void Dispose()
        {
            if (repositorySubscription != null)
                repositorySubscription.Dispose();
        }
    }
}
