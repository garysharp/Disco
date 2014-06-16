using Disco.Data.Repository;
using Disco.Data.Repository.Monitor;
using Disco.Models.Repository;
using Disco.Models.Services.Interop.ActiveDirectory;
using Disco.Services.Interop.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Disco.Services.Users.UserFlags
{
    public class UserFlagUserDevicesManagedGroup : ADManagedGroup
    {
        private const string KeyFormat = "UserFlag_{0}_UserDevices";
        private const string DescriptionFormat = "User associated with the {0} Flag will have their assigned Devices added to this Active Directory group.";
        private const string CategoryDescriptionFormat = "Assigned User Devices Linked Group";
        private const string GroupDescriptionFormat = "{0} [User Flag User Devices]";

        private IDisposable repositorySubscription;
        private int UserFlagId;
        private string UserFlagName;

        public override string Description { get { return string.Format(DescriptionFormat, UserFlagName); } }
        public override string CategoryDescription { get { return CategoryDescriptionFormat; } }
        public override string GroupDescription { get { return string.Format(GroupDescriptionFormat, UserFlagName); } }
        public override bool IncludeFilterBeginDate { get { return false; } }

        private UserFlagUserDevicesManagedGroup(string Key, ADManagedGroupConfiguration Configuration, UserFlag UserFlag)
            : base(Key, Configuration)
        {
            this.UserFlagId = UserFlag.Id;
            this.UserFlagName = UserFlag.Name;
        }

        public override void Initialize()
        {
            // Subscribe to changes
            repositorySubscription = UserFlagService.UserFlagAssignmentRepositoryEvents.Value
                .Where(e =>
                    (((UserFlagAssignment)e.Entity).UserFlagId == UserFlagId))
                .Subscribe(ProcessRepositoryEvent);
        }

        public static string GetKey(UserFlag UserFlag)
        {
            return string.Format(KeyFormat, UserFlag.Id);
        }
        public static string GetDescription(UserFlag UserFlag)
        {
            return string.Format(DescriptionFormat, UserFlag.Name);
        }
        public static string GetCategoryDescription(UserFlag UserFlag)
        {
            return CategoryDescriptionFormat;
        }

        public static bool TryGetManagedGroup(UserFlag UserFlag, out UserFlagUserDevicesManagedGroup ManagedGroup)
        {
            ADManagedGroup managedGroup;
            string key = GetKey(UserFlag);

            if (ActiveDirectory.Context.ManagedGroups.TryGetValue(key, out managedGroup))
            {
                ManagedGroup = (UserFlagUserDevicesManagedGroup)managedGroup;
                return true;
            }
            else
            {
                ManagedGroup = null;
                return false;
            }
        }

        public static UserFlagUserDevicesManagedGroup Initialize(UserFlag UserFlag)
        {
            if (UserFlag.Id > 0)
            {
                var key = GetKey(UserFlag);

                if (!string.IsNullOrEmpty(UserFlag.UserDevicesLinkedGroup))
                {
                    var config = ADManagedGroup.ConfigurationFromJson(UserFlag.UserDevicesLinkedGroup);

                    if (config != null && !string.IsNullOrWhiteSpace(config.GroupId))
                    {
                        var group = new UserFlagUserDevicesManagedGroup(
                            key,
                            config,
                            UserFlag);

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

        private IEnumerable<string> DetermineDeviceMembers(DiscoDataContext Database, string UserId)
        {
            return DetermineDeviceMembers(Database.Users.Where(u => u.UserId == UserId));
        }

        private IEnumerable<string> DetermineDeviceMembers(IQueryable<User> Users)
        {
            return Users
                .SelectMany(u => u.DeviceUserAssignments)
                .Where(da => !da.UnassignedDate.HasValue && da.Device.DeviceDomainId != null)
                .Select(da => da.Device.DeviceDomainId)
                .ToList()
                .Where(ActiveDirectory.IsValidDomainAccountId)
                .Select(id => id + "$");
        }

        public override IEnumerable<string> DetermineMembers(DiscoDataContext Database)
        {
            var assignments = Database.UserFlagAssignments
                .Where(a => a.UserFlagId == UserFlagId && !a.RemovedDate.HasValue)
                .Select(a => a.User);

            return DetermineDeviceMembers(assignments);
        }

        private void ProcessRepositoryEvent(RepositoryMonitorEvent Event)
        {
            var userFlagAssignemnt = (UserFlagAssignment)Event.Entity;
            string userId = userFlagAssignemnt.UserId;

            switch (Event.EventType)
            {
                case RepositoryMonitorEventType.Added:
                    if (!userFlagAssignemnt.RemovedDate.HasValue)
                        AddMember(userFlagAssignemnt.UserId, (database) => DetermineDeviceMembers(database, userId));
                    break;
                case RepositoryMonitorEventType.Modified:
                    if (userFlagAssignemnt.RemovedDate.HasValue)
                        RemoveMember(userFlagAssignemnt.UserId, (database) => DetermineDeviceMembers(database, userId));
                    else
                        AddMember(userFlagAssignemnt.UserId, (database) => DetermineDeviceMembers(database, userId));
                    break;
                case RepositoryMonitorEventType.Deleted:
                    // Remove the user's devices if no other (non-removed) assignments exist.
                    RemoveMember(userId, (database) =>
                    {
                        if (database.UserFlagAssignments.Any(a => a.UserFlagId == UserFlagId && a.UserId == userId && !a.RemovedDate.HasValue))
                            return null;
                        else
                            return DetermineDeviceMembers(database, userId);
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
