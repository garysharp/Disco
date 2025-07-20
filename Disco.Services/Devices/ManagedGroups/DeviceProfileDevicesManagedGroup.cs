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
    public class DeviceProfileDevicesManagedGroup : ADManagedGroup
    {
        private const string KeyFormat = "DeviceProfile_{0}_Devices";
        private const string DescriptionFormat = "Devices within the {0} Profile will be added to this Active Directory group.";
        private const string CategoryDescriptionFormat = "Devices Linked Group";
        private const string GroupDescriptionFormat = "{0} [Device Profile Devices]";

        private static Lazy<IObservable<RepositoryMonitorEvent>> RepositoryEvents;

        private IDisposable repositorySubscription;
        private int DeviceProfileId;
        private string DeviceProfileName;

        public override string Description { get { return string.Format(DescriptionFormat, DeviceProfileName); } }
        public override string CategoryDescription { get { return CategoryDescriptionFormat; } }
        public override string GroupDescription { get { return string.Format(GroupDescriptionFormat, DeviceProfileName); } }
        public override bool IncludeFilterBeginDate { get { return false; } }

        static DeviceProfileDevicesManagedGroup()
        {
            RepositoryEvents =
                new Lazy<IObservable<RepositoryMonitorEvent>>(() =>
                    RepositoryMonitor.StreamBeforeCommit.Where(e =>
                        e.EntityType == typeof(Device) && (
                        (e.EventType == RepositoryMonitorEventType.Added &&
                            ActiveDirectory.IsValidDomainAccountId(((Device)e.Entity).DeviceDomainId)) ||
                        (e.EventType == RepositoryMonitorEventType.Modified &&
                            (e.ModifiedProperties.Contains("DeviceProfileId") || e.ModifiedProperties.Contains("DeviceDomainId") || e.ModifiedProperties.Contains("LastEnrolDate"))) ||
                        (e.EventType == RepositoryMonitorEventType.Deleted)
                        )
                    ));
        }

        private DeviceProfileDevicesManagedGroup(string Key, ADManagedGroupConfiguration Configuration, DeviceProfile DeviceProfile)
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

        public static bool TryGetManagedGroup(DeviceProfile DeviceProfile, out DeviceProfileDevicesManagedGroup ManagedGroup)
        {
            string key = GetKey(DeviceProfile);

            if (ActiveDirectory.Context.ManagedGroups.TryGetValue(key, out var managedGroup))
            {
                ManagedGroup = (DeviceProfileDevicesManagedGroup)managedGroup;
                return true;
            }
            else
            {
                ManagedGroup = null;
                return false;
            }
        }

        public static DeviceProfileDevicesManagedGroup Initialize(DeviceProfile DeviceProfile)
        {
            if (DeviceProfile.Id > 0)
            {
                var key = GetKey(DeviceProfile);

                if (!string.IsNullOrEmpty(DeviceProfile.DevicesLinkedGroup))
                {
                    var config = ADManagedGroup.ConfigurationFromJson(DeviceProfile.DevicesLinkedGroup);

                    if (config != null && !string.IsNullOrWhiteSpace(config.GroupId))
                    {
                        var group = new DeviceProfileDevicesManagedGroup(
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
                        if (device.DeviceProfileId == DeviceProfileId)
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
