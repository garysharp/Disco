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
    public class DeviceFlagDeviceAssignedUsersManagedGroup : ADManagedGroup
    {
        private const string KeyFormat = "DeviceFlag_{0}_DeviceAssignedUsers";
        private const string DescriptionFormat = "User associated with devices which have the {0} Flag will be added to this Active Directory group.";
        private const string CategoryDescriptionFormat = "Device Assigned Users Linked Group";
        private const string GroupDescriptionFormat = "{0} [Device Flag Device Assigned Users]";

        private IDisposable repositorySubscription;
        private int deviceFlagId;
        private string deviceFlagName;

        public override string Description { get { return string.Format(DescriptionFormat, deviceFlagName); } }
        public override string CategoryDescription { get { return CategoryDescriptionFormat; } }
        public override string GroupDescription { get { return string.Format(GroupDescriptionFormat, deviceFlagName); } }
        public override bool IncludeFilterBeginDate { get { return true; } }

        private DeviceFlagDeviceAssignedUsersManagedGroup(string Key, ADManagedGroupConfiguration Configuration, DeviceFlag deviceFlag)
            : base(Key, Configuration)
        {
            deviceFlagId = deviceFlag.Id;
            deviceFlagName = deviceFlag.Name;
        }

        public override void Initialize()
        {
            // Subscribe to changes
            repositorySubscription = DeviceFlagService.DeviceFlagAssignmentRepositoryEvents.Value
                .Where(e =>
                    (((DeviceFlagAssignment)e.Entity).DeviceFlagId == deviceFlagId))
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

        public static bool TryGetManagedGroup(DeviceFlag deviceFlag, out DeviceFlagDeviceAssignedUsersManagedGroup managedGroup)
        {
            return ActiveDirectory.Context.ManagedGroups.TryGetValue(GetKey(deviceFlag), out managedGroup);
        }

        public static DeviceFlagDeviceAssignedUsersManagedGroup Initialize(DeviceFlag deviceFlag)
        {
            if (deviceFlag.Id > 0)
            {
                var key = GetKey(deviceFlag);

                if (!string.IsNullOrEmpty(deviceFlag.DeviceUsersLinkedGroup))
                {
                    var config = ConfigurationFromJson(deviceFlag.DeviceUsersLinkedGroup);

                    if (config != null && !string.IsNullOrWhiteSpace(config.GroupId))
                    {
                        var group = new DeviceFlagDeviceAssignedUsersManagedGroup(
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
            var query = (IQueryable<User>)Database.Users;

            if (Configuration.FilterBeginDate.HasValue)
            {
                query = query
                    .Where(u => u.DeviceUserAssignments.Any(a =>
                        a.UnassignedDate == null &&
                        a.Device.DeviceFlagAssignments.Any(fa =>
                            fa.DeviceFlagId == deviceFlagId &&
                            !fa.RemovedDate.HasValue &&
                            fa.AddedDate >= Configuration.FilterBeginDate)));
            }
            else
            {
                query = query
                    .Where(u => u.DeviceUserAssignments.Any(a =>
                        a.UnassignedDate == null &&
                        a.Device.DeviceFlagAssignments.Any(fa =>
                            fa.DeviceFlagId == deviceFlagId &&
                            !fa.RemovedDate.HasValue)));
            }

            return query.Select(u => u.UserId)
                .Distinct()
                .ToList()
                .Where(ActiveDirectory.IsValidDomainAccountId)
                .ToList();
        }

        private void ProcessRepositoryEvent(RepositoryMonitorEvent Event)
        {
            var assignment = (DeviceFlagAssignment)Event.Entity;

            string userId = assignment.Device?.AssignedUserId;
            if (!ActiveDirectory.IsValidDomainAccountId(userId))
                return;

            switch (Event.EventType)
            {
                case RepositoryMonitorEventType.Added:
                    if (Configuration.FilterBeginDate.HasValue)
                    {
                        if (!assignment.RemovedDate.HasValue && assignment.AddedDate >= Configuration.FilterBeginDate)
                        {
                            AddMember(userId);
                        }
                    }
                    else
                    {
                        if (!assignment.RemovedDate.HasValue)
                        {
                            AddMember(userId);
                        }
                    }
                    break;
                case RepositoryMonitorEventType.Modified:
                    if (!Configuration.FilterBeginDate.HasValue || assignment.AddedDate >= Configuration.FilterBeginDate)
                    {
                        if (assignment.RemovedDate.HasValue)
                            RemoveMember(userId, (database) =>
                            {
                                if (database.Users.Any(u => u.DeviceUserAssignments.Any(a =>
                                    a.UnassignedDate == null &&
                                    a.Device.DeviceFlagAssignments.Any(fa =>
                                        fa.DeviceFlagId == deviceFlagId &&
                                        !fa.RemovedDate.HasValue))))
                                {
                                    return null;
                                }
                                else
                                {
                                    return new[] { userId };
                                }
                            }
                            );
                        else
                            AddMember(userId);
                    }
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
