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
    public class DeviceBatchDevicesManagedGroup : ADManagedGroup
    {
        private const string KeyFormat = "DeviceBatch_{0}_Devices";
        private const string DescriptionFormat = "Devices within the {0} Batch will be added to this Active Directory group.";
        private const string CategoryDescriptionFormat = "Devices Linked Group";
        private const string GroupDescriptionFormat = "{0} [Device Batch Devices]";
        private static Lazy<IObservable<RepositoryMonitorEvent>> RepositoryEvents;

        private IDisposable repositorySubscription;
        private int DeviceBatchId;
        private string DeviceBatchName;

        public override string Description { get { return string.Format(DescriptionFormat, DeviceBatchName); } }
        public override string CategoryDescription { get { return CategoryDescriptionFormat; } }
        public override string GroupDescription { get { return string.Format(GroupDescriptionFormat, DeviceBatchName); } }
        public override bool IncludeFilterBeginDate { get { return false; } }

        static DeviceBatchDevicesManagedGroup()
        {
            RepositoryEvents =
                new Lazy<IObservable<RepositoryMonitorEvent>>(() =>
                    RepositoryMonitor.StreamBeforeCommit.Where(e =>
                        e.EntityType == typeof(Device) && (
                        (e.EventType == RepositoryMonitorEventType.Added &&
                            ActiveDirectory.IsValidDomainAccountId(((Device)e.Entity).DeviceDomainId)) ||
                        (e.EventType == RepositoryMonitorEventType.Modified &&
                            (e.ModifiedProperties.Contains("DeviceBatchId") || e.ModifiedProperties.Contains("DeviceDomainId") || e.ModifiedProperties.Contains("LastEnrolDate"))) ||
                        (e.EventType == RepositoryMonitorEventType.Deleted)
                        )
                    ));
        }

        private DeviceBatchDevicesManagedGroup(string Key, ADManagedGroupConfiguration Configuration, DeviceBatch DeviceBatch)
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

        public static bool TryGetManagedGroup(DeviceBatch DeviceBatch, out DeviceBatchDevicesManagedGroup ManagedGroup)
        {
            ADManagedGroup managedGroup;
            string key = GetKey(DeviceBatch);

            if (ActiveDirectory.Context.ManagedGroups.TryGetValue(key, out managedGroup))
            {
                ManagedGroup = (DeviceBatchDevicesManagedGroup)managedGroup;
                return true;
            }
            else
            {
                ManagedGroup = null;
                return false;
            }
        }

        public static DeviceBatchDevicesManagedGroup Initialize(DeviceBatch DeviceBatch)
        {
            if (DeviceBatch.Id > 0)
            {
                var key = GetKey(DeviceBatch);

                if (!string.IsNullOrEmpty(DeviceBatch.DevicesLinkedGroup))
                {
                    var config = ADManagedGroup.ConfigurationFromJson(DeviceBatch.DevicesLinkedGroup);

                    if (config != null && !string.IsNullOrWhiteSpace(config.GroupId))
                    {
                        var group = new DeviceBatchDevicesManagedGroup(
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
                .Where(d => d.DeviceDomainId != null)
                .Select(d => d.DeviceDomainId)
                .ToList()
                .Where(ActiveDirectory.IsValidDomainAccountId)
                .Select(id => id + "$");
        }

        private void ProcessRepositoryEvent(RepositoryMonitorEvent Event)
        {
            var device = (Device)Event.Entity;
            string previousDeviceDomainId = Event.GetPreviousPropertyValue<string>("DeviceDomainId");

            Event.ExecuteAfterCommit(e =>
            {
                switch (e.EventType)
                {
                    case RepositoryMonitorEventType.Added:
                        AddMember(device.DeviceDomainId + "$");
                        break;
                    case RepositoryMonitorEventType.Modified:
                        if (device.DeviceBatchId == DeviceBatchId)
                        {
                            if (ActiveDirectory.IsValidDomainAccountId(device.DeviceDomainId))
                                AddMember(device.DeviceDomainId + "$");

                            if (e.ModifiedProperties.Contains("DeviceDomainId"))
                            {
                                if (ActiveDirectory.IsValidDomainAccountId(previousDeviceDomainId))
                                    RemoveMember(previousDeviceDomainId + "$");
                            }
                        }
                        else
                        {
                            if (e.ModifiedProperties.Contains("DeviceDomainId"))
                            {
                                if (ActiveDirectory.IsValidDomainAccountId(previousDeviceDomainId))
                                    RemoveMember(previousDeviceDomainId + "$");
                            }
                            else
                            {
                                if (ActiveDirectory.IsValidDomainAccountId(device.DeviceDomainId))
                                    RemoveMember(device.DeviceDomainId + "$");
                            }
                        }
                        break;
                    case RepositoryMonitorEventType.Deleted:
                        if (ActiveDirectory.IsValidDomainAccountId(previousDeviceDomainId))
                            RemoveMember(previousDeviceDomainId + "$");
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
