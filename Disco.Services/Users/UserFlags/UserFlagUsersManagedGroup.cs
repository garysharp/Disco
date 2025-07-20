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
    public class UserFlagUsersManagedGroup : ADManagedGroup
    {
        private const string KeyFormat = "UserFlag_{0}_Users";
        private const string DescriptionFormat = "User associated with the {0} Flag will be added to this Active Directory group.";
        private const string CategoryDescriptionFormat = "Assigned Users Linked Group";
        private const string GroupDescriptionFormat = "{0} [User Flag Users]";

        private IDisposable repositorySubscription;
        private int UserFlagId;
        private string UserFlagName;

        public override string Description { get { return string.Format(DescriptionFormat, UserFlagName); } }
        public override string CategoryDescription { get { return CategoryDescriptionFormat; } }
        public override string GroupDescription { get { return string.Format(GroupDescriptionFormat, UserFlagName); } }
        public override bool IncludeFilterBeginDate { get { return true; } }

        private UserFlagUsersManagedGroup(string Key, ADManagedGroupConfiguration Configuration, UserFlag UserFlag)
            : base(Key, Configuration)
        {
            UserFlagId = UserFlag.Id;
            UserFlagName = UserFlag.Name;
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

        public static bool TryGetManagedGroup(UserFlag UserFlag, out UserFlagUsersManagedGroup ManagedGroup)
        {
            string key = GetKey(UserFlag);

            if (ActiveDirectory.Context.ManagedGroups.TryGetValue(key, out var managedGroup))
            {
                ManagedGroup = (UserFlagUsersManagedGroup)managedGroup;
                return true;
            }
            else
            {
                ManagedGroup = null;
                return false;
            }
        }

        public static UserFlagUsersManagedGroup Initialize(UserFlag UserFlag)
        {
            if (UserFlag.Id > 0)
            {
                var key = GetKey(UserFlag);

                if (!string.IsNullOrEmpty(UserFlag.UsersLinkedGroup))
                {
                    var config = ConfigurationFromJson(UserFlag.UsersLinkedGroup);

                    if (config != null && !string.IsNullOrWhiteSpace(config.GroupId))
                    {
                        var group = new UserFlagUsersManagedGroup(
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

        public override IEnumerable<string> DetermineMembers(DiscoDataContext Database)
        {
            if (Configuration.FilterBeginDate.HasValue)
            {
                return Database.UserFlagAssignments
                    .Where(a => a.UserFlagId == UserFlagId && !a.RemovedDate.HasValue && a.AddedDate >= Configuration.FilterBeginDate)
                    .Select(a => a.UserId);
            }
            else
            {
                return Database.UserFlagAssignments
                    .Where(a => a.UserFlagId == UserFlagId && !a.RemovedDate.HasValue)
                    .Select(a => a.UserId);
            }
        }

        private void ProcessRepositoryEvent(RepositoryMonitorEvent Event)
        {
            var userFlagAssignment = (UserFlagAssignment)Event.Entity;

            switch (Event.EventType)
            {
                case RepositoryMonitorEventType.Added:
                    if (Configuration.FilterBeginDate.HasValue)
                    {
                        if (!userFlagAssignment.RemovedDate.HasValue && userFlagAssignment.AddedDate >= Configuration.FilterBeginDate)
                        {
                            AddMember(userFlagAssignment.UserId);
                        }
                    }
                    else
                    {
                        if (!userFlagAssignment.RemovedDate.HasValue)
                        {
                            AddMember(userFlagAssignment.UserId);
                        }
                    }
                    break;
                case RepositoryMonitorEventType.Modified:
                    if (Configuration.FilterBeginDate.HasValue)
                    {
                        if (userFlagAssignment.AddedDate >= Configuration.FilterBeginDate)
                        {
                            if (userFlagAssignment.RemovedDate.HasValue)
                            {
                                RemoveMember(userFlagAssignment.UserId);
                            }
                            else
                            {
                                AddMember(userFlagAssignment.UserId);
                            }
                        }
                    }
                    else
                    {
                        if (userFlagAssignment.RemovedDate.HasValue)
                        {
                            RemoveMember(userFlagAssignment.UserId);
                        }
                        else
                        {
                            AddMember(userFlagAssignment.UserId);
                        }
                    }
                    break;
                case RepositoryMonitorEventType.Deleted:
                    string userId = userFlagAssignment.UserId;
                    // Remove the user if no other (non-removed) assignments exist.
                    RemoveMember(userId, (database) =>
                    {
                        if (Configuration.FilterBeginDate.HasValue)
                        {
                            if (database.UserFlagAssignments.Any(a => a.UserFlagId == UserFlagId && a.UserId == userId && !a.RemovedDate.HasValue && a.AddedDate >= Configuration.FilterBeginDate))
                            {
                                return null;
                            }
                            else
                            {
                                return new string[] { userId };
                            }
                        }
                        else
                        {
                            if (database.UserFlagAssignments.Any(a => a.UserFlagId == UserFlagId && a.UserId == userId && !a.RemovedDate.HasValue))
                            {
                                return null;
                            }
                            else
                            {
                                return new string[] { userId };
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
