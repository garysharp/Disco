using Disco.Data.Repository;
using Disco.Data.Repository.Monitor;
using Disco.Models.Repository;
using Disco.Models.Services.Interop.ActiveDirectory;
using Disco.Services.Interop.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Disco.Services.Devices.ManagedGroups
{
    public class DeviceProfileAssignedUsersManagedGroup : ADManagedGroup
    {
        private const string KeyFormat = "DeviceProfile_{0}_AssignedUsers";
        private const string DescriptionFormat = "Devices within the {0} Profile will have their assigned users added to this Active Directory group.";
        private const string CategoryDescriptionFormat = "Assigned Users Linked Group";
        private const string GroupDescriptionFormat = "{0} [Device Profile Assigned Users]";

        private static Lazy<IObservable<RepositoryMonitorEvent>> RepositoryEvents;

        private IDisposable repositorySubscription;
        private int DeviceProfileId;
        private string DeviceProfileName;

        public override string Description { get { return string.Format(DescriptionFormat, DeviceProfileName); } }
        public override string CategoryDescription { get { return CategoryDescriptionFormat; } }
        public override string GroupDescription { get { return string.Format(GroupDescriptionFormat, DeviceProfileName); } }
        public override bool IncludeFilterBeginDate { get { return false; } }

        static DeviceProfileAssignedUsersManagedGroup()
        {
            RepositoryEvents =
                new Lazy<IObservable<RepositoryMonitorEvent>>(() =>
                    RepositoryMonitor.StreamBeforeCommit.Where(e =>
                        e.EntityType == typeof(Device) && (
                        (e.EventType == RepositoryMonitorEventType.Added &&
                            ((Device)e.Entity).AssignedUserId != null) ||
                        (e.EventType == RepositoryMonitorEventType.Modified &&
                            (e.ModifiedProperties.Contains("DeviceProfileId") || e.ModifiedProperties.Contains("AssignedUserId"))) ||
                        (e.EventType == RepositoryMonitorEventType.Deleted &&
                            ((Device)e.Entity).AssignedUserId != null))
                        )
                    );
        }

        private DeviceProfileAssignedUsersManagedGroup(string Key, ADManagedGroupConfiguration Configuration, DeviceProfile DeviceProfile)
            : base(Key, Configuration)
        {
            DeviceProfileId = DeviceProfile.Id;
            DeviceProfileName = DeviceProfile.Name;
        }

        public override void Initialize()
        {
            // Subscribe to changes
            repositorySubscription = RepositoryEvents.Value
                .Where(e =>
                    (((Device)e.Entity).DeviceProfileId == DeviceProfileId) ||
                    (e.EventType == RepositoryMonitorEventType.Modified && e.GetPreviousPropertyValue<int>("DeviceProfileId") == DeviceProfileId))
                .Subscribe(ProcessRepositoryEvent);
        }

        public static string GetKey(DeviceProfile DeviceProfile)
        {
            return string.Format(KeyFormat, DeviceProfile.Id);
        }
        public static string GetDescription(DeviceProfile DeviceProfile)
        {
            return string.Format(DescriptionFormat, DeviceProfile.Name);
        }
        public static string GetCategoryDescription(DeviceProfile DeviceProfile)
        {
            return CategoryDescriptionFormat;
        }

        public static bool TryGetManagedGroup(DeviceProfile DeviceProfile, out DeviceProfileAssignedUsersManagedGroup ManagedGroup)
        {
            string key = GetKey(DeviceProfile);

            if (ActiveDirectory.Context.ManagedGroups.TryGetValue(key, out var managedGroup))
            {
                ManagedGroup = (DeviceProfileAssignedUsersManagedGroup)managedGroup;
                return true;
            }
            else
            {
                ManagedGroup = null;
                return false;
            }
        }

        public static DeviceProfileAssignedUsersManagedGroup Initialize(DeviceProfile DeviceProfile)
        {
            if (DeviceProfile.Id > 0)
            {
                var key = GetKey(DeviceProfile);

                if (!string.IsNullOrEmpty(DeviceProfile.AssignedUsersLinkedGroup))
                {
                    var config = ConfigurationFromJson(DeviceProfile.AssignedUsersLinkedGroup);

                    if (config != null && !string.IsNullOrWhiteSpace(config.GroupId))
                    {
                        var group = new DeviceProfileAssignedUsersManagedGroup(
                            key,
                            config,
                            DeviceProfile);

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
            return Database.Devices
                .Where(d => d.DeviceProfileId == DeviceProfileId)
                .Where(d => d.AssignedUserId != null)
                .Select(d => d.AssignedUserId);
        }

        private void ProcessRepositoryEvent(RepositoryMonitorEvent Event)
        {
            var device = (Device)Event.Entity;
            string previousUserId = Event.GetPreviousPropertyValue<string>("AssignedUserId");

            Event.ExecuteAfterCommit(e =>
            {
                switch (e.EventType)
                {
                    case RepositoryMonitorEventType.Added:
                        AddMember(device.AssignedUserId);
                        break;
                    case RepositoryMonitorEventType.Modified:
                        if (device.DeviceProfileId == DeviceProfileId)
                        {
                            if (device.AssignedUserId != null)
                                AddMember(device.AssignedUserId);

                            if (e.ModifiedProperties.Contains("AssignedUserId"))
                            {
                                if (previousUserId != null)
                                    RemoveMember(previousUserId, (database) =>
                                        !database.Devices.Any(d => d.DeviceProfileId == DeviceProfileId && d.AssignedUserId == previousUserId)
                                            ? new string[] { previousUserId }
                                            : null);
                            }
                        }
                        else
                        {
                            if (previousUserId != null)
                                RemoveMember(previousUserId, (database) =>
                                    !database.Devices.Any(d => d.DeviceProfileId == DeviceProfileId && d.AssignedUserId == previousUserId)
                                        ? new string[] { previousUserId }
                                        : null);
                        }
                        break;
                    case RepositoryMonitorEventType.Deleted:
                        if (previousUserId != null)
                            RemoveMember(previousUserId, (database) =>
                                !database.Devices.Any(d => d.DeviceProfileId == DeviceProfileId && d.AssignedUserId == previousUserId)
                                    ? new string[] { previousUserId }
                                    : null);
                        break;
                }
            });
        }

        public override void Dispose()
        {
            if (repositorySubscription != null)
                repositorySubscription.Dispose();
        }
    }
}
