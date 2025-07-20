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
    public class DeviceBatchAssignedUsersManagedGroup : ADManagedGroup
    {
        private const string KeyFormat = "DeviceBatch_{0}_AssignedUsers";
        private const string DescriptionFormat = "Devices within the {0} Batch will have their assigned users added to this Active Directory group.";
        private const string CategoryDescriptionFormat = "Assigned Users Linked Group";
        private const string GroupDescriptionFormat = "{0} [Device Batch Assigned Users]";

        private static Lazy<IObservable<RepositoryMonitorEvent>> RepositoryEvents;

        private IDisposable repositorySubscription;
        private int DeviceBatchId;
        private string DeviceBatchName;

        public override string Description { get { return string.Format(DescriptionFormat, DeviceBatchName); } }
        public override string CategoryDescription { get { return CategoryDescriptionFormat; } }
        public override string GroupDescription { get { return string.Format(GroupDescriptionFormat, DeviceBatchName); } }
        public override bool IncludeFilterBeginDate { get { return false; } }

        static DeviceBatchAssignedUsersManagedGroup()
        {
            RepositoryEvents =
                new Lazy<IObservable<RepositoryMonitorEvent>>(() =>
                    RepositoryMonitor.StreamBeforeCommit.Where(e =>
                        e.EntityType == typeof(Device) && (
                        (e.EventType == RepositoryMonitorEventType.Added &&
                            ((Device)e.Entity).AssignedUserId != null) ||
                        (e.EventType == RepositoryMonitorEventType.Modified &&
                            (e.ModifiedProperties.Contains("DeviceBatchId") || e.ModifiedProperties.Contains("AssignedUserId"))) ||
                        (e.EventType == RepositoryMonitorEventType.Deleted &&
                            ((Device)e.Entity).AssignedUserId != null))
                        )
                    );
        }

        private DeviceBatchAssignedUsersManagedGroup(string Key, ADManagedGroupConfiguration Configuration, DeviceBatch DeviceBatch)
            : base(Key, Configuration)
        {
            DeviceBatchId = DeviceBatch.Id;
            DeviceBatchName = DeviceBatch.Name;
        }

        public override void Initialize()
        {
            // Subscribe to changes
            repositorySubscription = RepositoryEvents.Value
                .Where(e =>
                    (((Device)e.Entity).DeviceBatchId == DeviceBatchId) ||
                    (e.EventType == RepositoryMonitorEventType.Modified && e.GetPreviousPropertyValue<int?>("DeviceBatchId") == DeviceBatchId))
                .Subscribe(ProcessRepositoryEvent);
        }

        public static string GetKey(DeviceBatch DeviceBatch)
        {
            return string.Format(KeyFormat, DeviceBatch.Id);
        }
        public static string GetDescription(DeviceBatch DeviceBatch)
        {
            return string.Format(DescriptionFormat, DeviceBatch.Name);
        }
        public static string GetCategoryDescription(DeviceBatch DeviceBatch)
        {
            return CategoryDescriptionFormat;
        }

        public static bool TryGetManagedGroup(DeviceBatch DeviceBatch, out DeviceBatchAssignedUsersManagedGroup ManagedGroup)
        {
            ADManagedGroup managedGroup;
            string key = GetKey(DeviceBatch);

            if (ActiveDirectory.Context.ManagedGroups.TryGetValue(key, out managedGroup))
            {
                ManagedGroup = (DeviceBatchAssignedUsersManagedGroup)managedGroup;
                return true;
            }
            else
            {
                ManagedGroup = null;
                return false;
            }
        }

        public static DeviceBatchAssignedUsersManagedGroup Initialize(DeviceBatch DeviceBatch)
        {
            if (DeviceBatch.Id > 0)
            {
                var key = GetKey(DeviceBatch);

                if (!string.IsNullOrEmpty(DeviceBatch.AssignedUsersLinkedGroup))
                {
                    var config = ADManagedGroup.ConfigurationFromJson(DeviceBatch.AssignedUsersLinkedGroup);

                    if (config != null && !string.IsNullOrWhiteSpace(config.GroupId))
                    {
                        var group = new DeviceBatchAssignedUsersManagedGroup(
                            key,
                            config,
                            DeviceBatch);

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
                .Where(d => d.DeviceBatchId == DeviceBatchId)
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
                        if (device.DeviceBatchId == DeviceBatchId)
                        {
                            if (device.AssignedUserId != null)
                                AddMember(device.AssignedUserId);

                            if (e.ModifiedProperties.Contains("AssignedUserId"))
                            {
                                if (previousUserId != null)
                                    RemoveMember(previousUserId, (database) =>
                                        !database.Devices.Any(d => d.DeviceBatchId == DeviceBatchId && d.AssignedUserId == previousUserId)
                                            ? new string[] { previousUserId }
                                            : null);
                            }
                        }
                        else
                        {
                            if (previousUserId != null)
                                RemoveMember(previousUserId, (database) =>
                                    !database.Devices.Any(d => d.DeviceBatchId == DeviceBatchId && d.AssignedUserId == previousUserId)
                                        ? new string[] { previousUserId }
                                        : null);
                        }
                        break;
                    case RepositoryMonitorEventType.Deleted:
                        if (previousUserId != null)
                            RemoveMember(previousUserId, (database) =>
                                !database.Devices.Any(d => d.DeviceBatchId == DeviceBatchId && d.AssignedUserId == previousUserId)
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
