using Disco.Data.Repository;
using Disco.Data.Repository.Monitor;
using Disco.Models.Repository;
using Disco.Services.Extensions;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Disco.Services.Users.UserFlags
{
    public static class UserFlagService
    {
        private static Cache _cache;
        internal static Lazy<IObservable<RepositoryMonitorEvent>> UserFlagAssignmentRepositoryEvents;

        static UserFlagService()
        {
            // Statically defined (lazy) Assignment Repository Definition
            // Used by UserFlagAssignedUsersManagedGroup & UserFlagAssignedUserDevicesManagedGroup
            UserFlagAssignmentRepositoryEvents =
                new Lazy<IObservable<RepositoryMonitorEvent>>(() =>
                    RepositoryMonitor.StreamAfterCommit.Where(e =>
                        e.EntityType == typeof(UserFlagAssignment) &&
                        (e.EventType != RepositoryMonitorEventType.Modified ||
                        e.ModifiedProperties.Contains("RemovedDate"))
                        )
                    );
        }

        public static void Initialize(DiscoDataContext Database)
        {
            _cache = new Cache(Database);

            // Initialize Managed Groups (if configured)
            _cache.GetUserFlags().ForEach(uf =>
            {
                UserFlagUsersManagedGroup.Initialize(uf);
                UserFlagUserDevicesManagedGroup.Initialize(uf);
            });
        }

        public static List<UserFlag> GetUserFlags() { return _cache.GetUserFlags(); }
        public static UserFlag GetUserFlag(int UserFlagId) { return _cache.GetUserFlag(UserFlagId); }

        #region User Flag Maintenance
        public static UserFlag CreateUserFlag(DiscoDataContext Database, UserFlag UserFlag)
        {
            // Verify
            if (string.IsNullOrWhiteSpace(UserFlag.Name))
                throw new ArgumentException("The User Flag Name is required");

            // Name Unique
            if (_cache.GetUserFlags().Any(f => f.Name == UserFlag.Name))
                throw new ArgumentException("Another User Flag already exists with that name", "UserFlag");

            // Clone to break reference
            var flag = new UserFlag()
            {
                Name = UserFlag.Name,
                Description = UserFlag.Description,
                Icon = UserFlag.Icon,
                IconColour = UserFlag.IconColour,
                UsersLinkedGroup = UserFlag.UsersLinkedGroup,
                UserDevicesLinkedGroup = UserFlag.UserDevicesLinkedGroup
            };

            Database.UserFlags.Add(flag);
            Database.SaveChanges();

            _cache.AddOrUpdate(flag);

            return flag;
        }
        public static UserFlag Update(DiscoDataContext Database, UserFlag UserFlag)
        {
            // Verify
            if (string.IsNullOrWhiteSpace(UserFlag.Name))
                throw new ArgumentException("The User Flag Name is required");

            // Name Unique
            if (_cache.GetUserFlags().Any(f => f.Id != UserFlag.Id && f.Name == UserFlag.Name))
                throw new ArgumentException("Another User Flag already exists with that name", "UserFlag");

            Database.SaveChanges();

            _cache.AddOrUpdate(UserFlag);
            UserFlagUsersManagedGroup.Initialize(UserFlag);
            UserFlagUserDevicesManagedGroup.Initialize(UserFlag);

            return UserFlag;
        }
        public static void DeleteUserFlag(DiscoDataContext Database, int UserFlagId, IScheduledTaskStatus Status)
        {
            UserFlag flag = Database.UserFlags.Find(UserFlagId);

            // Dispose of AD Managed Groups
            Interop.ActiveDirectory.ActiveDirectory.Context.ManagedGroups.Remove(UserFlagUserDevicesManagedGroup.GetKey(flag));
            Interop.ActiveDirectory.ActiveDirectory.Context.ManagedGroups.Remove(UserFlagUsersManagedGroup.GetKey(flag));

            // Delete Assignments
            Status.UpdateStatus(0, string.Format("Removing '{0}' [{1}] User Flag", flag.Name, flag.Id), "Starting");
            List<UserFlagAssignment> flagAssignments = Database.UserFlagAssignments.Where(fa => fa.UserFlagId == flag.Id).ToList();
            if (flagAssignments.Count > 0)
            {
                Status.UpdateStatus(20, "Removing flag from users");
                flagAssignments.ForEach(flagAssignment => Database.UserFlagAssignments.Remove(flagAssignment));
                Database.SaveChanges();
            }

            // Delete Flag
            Status.UpdateStatus(90, "Deleting User Flag");
            Database.UserFlags.Remove(flag);
            Database.SaveChanges();

            // Remove from Cache
            _cache.Remove(UserFlagId);

            Status.Finished(string.Format("Successfully Deleted User Flag: '{0}' [{1}]", flag.Name, flag.Id));
        }
        #endregion

        #region Bulk Assignment
        public static IEnumerable<UserFlagAssignment> BulkAssignAddUsers(DiscoDataContext Database, UserFlag UserFlag, User Technician, string Comments, List<User> Users, IScheduledTaskStatus Status)
        {
            if (Users.Count > 0)
            {
                double progressInterval;
                const int databaseChunkSize = 100;
                string comments = string.IsNullOrWhiteSpace(Comments) ? null : Comments.Trim();

                var addUsers = Users.Where(u => !u.UserFlagAssignments.Any(a => a.UserFlagId == UserFlag.Id && !a.RemovedDate.HasValue)).ToList();

                progressInterval = (double)100 / addUsers.Count;

                var addedUserAssignments = addUsers.Chunk(databaseChunkSize).SelectMany((chunk, chunkIndex) =>
                {
                    var chunkIndexOffset = databaseChunkSize * chunkIndex;

                    var chunkResults = chunk.Select((user, index) =>
                    {
                        Status.UpdateStatus((chunkIndexOffset + index) * progressInterval, string.Format("Assigning Flag: {0}", user.ToString()));

                        return user.OnAddUserFlag(Database, UserFlag, Technician, comments);
                    }).ToList();

                    // Save Chunk Items to Database
                    Database.SaveChanges();

                    return chunkResults;
                }).Where(fa => fa != null).ToList();

                Status.SetFinishedMessage(string.Format("{0} Users/s Added; {1} User/s Skipped", addUsers.Count, (Users.Count - addUsers.Count)));

                return addedUserAssignments;
            }
            else
            {
                Status.SetFinishedMessage("No changes found");
                return Enumerable.Empty<UserFlagAssignment>();
            }
        }

        public static IEnumerable<UserFlagAssignment> BulkAssignOverrideUsers(DiscoDataContext Database, UserFlag UserFlag, User Technician, string Comments, List<User> Users, IScheduledTaskStatus Status)
        {
            double progressInterval;
            const int databaseChunkSize = 100;
            string comments = string.IsNullOrWhiteSpace(Comments) ? null : Comments.Trim();

            Status.UpdateStatus(0, "Calculating assignment changes");

            var currentAssignments = Database.UserFlagAssignments.Include("User").Where(a => a.UserFlagId == UserFlag.Id && !a.RemovedDate.HasValue).ToList();
            var removeAssignments = currentAssignments.Where(ca => !Users.Any(u => u.UserId.Equals(ca.UserId, StringComparison.OrdinalIgnoreCase))).ToList();
            var addUsers = Users.Where(u => !currentAssignments.Any(ca => ca.UserId.Equals(u.UserId, StringComparison.OrdinalIgnoreCase))).ToList();

            if (removeAssignments.Count > 0 || addUsers.Count > 0)
            {
                progressInterval = (double)100 / (removeAssignments.Count + addUsers.Count);
                var removedDateTime = DateTime.Now;

                // Remove Assignments
                removeAssignments.Chunk(databaseChunkSize).SelectMany((chunk, chunkIndex) =>
                {
                    var chunkIndexOffset = (chunkIndex * databaseChunkSize) + removeAssignments.Count;

                    var chunkResults = chunk.Select((flagAssignment, index) =>
                    {
                        Status.UpdateStatus((chunkIndexOffset + index) * progressInterval, string.Format("Removing Flag: {0}", flagAssignment.User.ToString()));

                        flagAssignment.OnRemoveUnsafe(Database, Technician);
                        
                        return flagAssignment;
                    }).ToList();

                    // Save Chunk Items to Database
                    Database.SaveChanges();

                    return chunkResults;
                }).ToList();

                // Add Assignments
                var addedUserAssignments = addUsers.Chunk(databaseChunkSize).SelectMany((chunk, chunkIndex) =>
                {
                    var chunkIndexOffset = (chunkIndex * databaseChunkSize) + removeAssignments.Count;

                    var chunkResults = chunk.Select((user, index) =>
                    {
                        Status.UpdateStatus((chunkIndexOffset + index) * progressInterval, string.Format("Assigning Flag: {0}", user.ToString()));

                        return user.OnAddUserFlag(Database, UserFlag, Technician, comments);
                    }).ToList();

                    // Save Chunk Items to Database
                    Database.SaveChanges();

                    return chunkResults;
                }).ToList();

                Status.SetFinishedMessage(string.Format("{0} Users/s Added; {1} User/s Removed; {2} User/s Skipped", addUsers.Count, removeAssignments.Count, (Users.Count - addUsers.Count)));

                return addedUserAssignments;
            }
            else
            {
                Status.SetFinishedMessage("No changes found");
                return Enumerable.Empty<UserFlagAssignment>();
            }
        }
        #endregion

        public static string RandomUnusedIcon()
        {
            return UIHelpers.RandomIcon(_cache.GetUserFlags().Select(f => f.Icon));
        }
        public static string RandomUnusedThemeColour()
        {
            return UIHelpers.RandomThemeColour(_cache.GetUserFlags().Select(f => f.IconColour));
        }
    }
}
