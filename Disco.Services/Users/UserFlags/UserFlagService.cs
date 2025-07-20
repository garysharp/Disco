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

namespace Disco.Services.Users.UserFlags
{
    public static class UserFlagService
    {
        private static Cache cache;
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
                        e.ModifiedProperties.Contains(nameof(UserFlagAssignment.RemovedDate)))
                        )
                    );
        }

        public static void Initialize(DiscoDataContext Database)
        {
            cache = new Cache(Database);

            // Initialize Managed Groups (if configured)
            cache.GetUserFlags().ForEach(uf =>
            {
                UserFlagUsersManagedGroup.Initialize(uf.flag);
                UserFlagUserDevicesManagedGroup.Initialize(uf.flag);
            });
        }

        public static IEnumerable<(UserFlag flag, FlagPermission permission)> GetUserFlags() { return cache.GetUserFlags(); }
        public static (UserFlag flag, FlagPermission permission) GetUserFlag(int UserFlagId) { return cache.GetUserFlag(UserFlagId); }

        public static UserFlag GetAvailableUserFlag(int userFlagId, User targetUser)
        {
            var (userFlag, permission) = cache.GetUserFlag(userFlagId);

            if (targetUser.UserFlagAssignments
                .Where(a => a.UserFlagId == userFlagId && !a.RemovedDate.HasValue).Any())
                return null;

            if (permission.CanAssign())
                return userFlag;

            return null;
        }

        public static IEnumerable<UserFlag> GetAvailableUserFlags(User targetUser)
        {
            var records = cache.GetUserFlags();

            var usedFlags = targetUser.UserFlagAssignments
                    .Where(a => !a.RemovedDate.HasValue)
                    .Select(a => a.UserFlagId)
                    .ToList();

            foreach (var (flag, permission) in records)
            {
                if (usedFlags.Contains(flag.Id))
                    continue;

                if (permission.CanAssign())
                    yield return flag;
            }
        }

        #region User Flag Maintenance
        public static UserFlag CreateUserFlag(DiscoDataContext Database, string name, string description)
        {
            // Verify
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The User Flag Name is required", nameof(name));

            // Name Unique
            if (cache.GetUserFlags().Any(f => f.flag.Name.Equals(name, StringComparison.Ordinal)))
                throw new ArgumentException("Another User Flag already exists with that name", nameof(name));

            // Clone to break reference
            var flag = new UserFlag()
            {
                Name = name,
                Description = description,
                Icon = RandomUnusedIcon(),
                IconColour = RandomUnusedThemeColour(),
            };

            Database.UserFlags.Add(flag);
            Database.SaveChanges();

            cache.AddOrUpdate(flag);

            return flag;
        }
        public static UserFlag Update(DiscoDataContext Database, UserFlag UserFlag)
        {
            // Verify
            if (string.IsNullOrWhiteSpace(UserFlag.Name))
                throw new ArgumentException("The User Flag Name is required");

            // Name Unique
            if (cache.GetUserFlags().Any(f => f.flag.Id != UserFlag.Id && f.flag.Name == UserFlag.Name))
                throw new ArgumentException("Another User Flag already exists with that name", nameof(UserFlag));

            Database.SaveChanges();

            cache.AddOrUpdate(UserFlag);
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
            Status.UpdateStatus(0, $"Removing '{flag.Name}' [{flag.Id}] User Flag", "Starting");
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
            cache.Remove(UserFlagId);

            Status.Finished($"Successfully Deleted User Flag: '{flag.Name}' [{flag.Id}]");
        }
        #endregion

        #region Bulk Assignment
        public static IEnumerable<UserFlagAssignment> BulkAssignAddUsers(DiscoDataContext database, UserFlag userFlag, User techUser, string comments, List<User> users, IScheduledTaskStatus status)
        {
            if (users.Count > 0)
            {
                double progressInterval;
                const int databaseChunkSize = 100;
                comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();

                var addUsers = users.Where(u => !u.UserFlagAssignments.Any(a => a.UserFlagId == userFlag.Id && !a.RemovedDate.HasValue)).ToList();

                progressInterval = (double)100 / addUsers.Count;

                var addedUserAssignments = addUsers.Chunk(databaseChunkSize).SelectMany((chunk, chunkIndex) =>
                {
                    var chunkIndexOffset = databaseChunkSize * chunkIndex;

                    var chunkResults = chunk.Select((user, index) =>
                    {
                        status.UpdateStatus((chunkIndexOffset + index) * progressInterval, $"Assigning Flag: {user}");

                        return user.OnAddUserFlagUnsafe(database, userFlag, techUser, comments);
                    }).ToList();

                    // Save Chunk Items to Database
                    database.SaveChanges();

                    return chunkResults;
                }).Where(fa => fa != null).ToList();

                status.SetFinishedMessage($"{addUsers.Count} Users/s Added; {users.Count - addUsers.Count} User/s Skipped");

                return addedUserAssignments;
            }
            else
            {
                status.SetFinishedMessage("No changes found");
                return Enumerable.Empty<UserFlagAssignment>();
            }
        }

        public static IEnumerable<UserFlagAssignment> BulkAssignOverrideUsers(DiscoDataContext database, UserFlag userFlag, User techUser, string comments, List<User> users, IScheduledTaskStatus status)
        {
            double progressInterval;
            const int databaseChunkSize = 100;
            comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();

            status.UpdateStatus(0, "Calculating assignment changes");

            var currentAssignments = database.UserFlagAssignments.Include(a => a.User).Where(a => a.UserFlagId == userFlag.Id && !a.RemovedDate.HasValue).ToList();
            var removeAssignments = currentAssignments.Where(ca => !users.Any(u => u.UserId.Equals(ca.UserId, StringComparison.OrdinalIgnoreCase))).ToList();
            var addUsers = users.Where(u => !currentAssignments.Any(ca => ca.UserId.Equals(u.UserId, StringComparison.OrdinalIgnoreCase))).ToList();

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
                        status.UpdateStatus((chunkIndexOffset + index) * progressInterval, $"Removing Flag: {flagAssignment.User}");

                        flagAssignment.OnRemoveUnsafe(database, techUser);

                        return flagAssignment;
                    }).ToList();

                    // Save Chunk Items to Database
                    database.SaveChanges();

                    return chunkResults;
                }).ToList();

                // Add Assignments
                var addedUserAssignments = addUsers.Chunk(databaseChunkSize).SelectMany((chunk, chunkIndex) =>
                {
                    var chunkIndexOffset = (chunkIndex * databaseChunkSize) + removeAssignments.Count;

                    var chunkResults = chunk.Select((user, index) =>
                    {
                        status.UpdateStatus((chunkIndexOffset + index) * progressInterval, string.Format("Assigning Flag: {0}", user.ToString()));

                        return user.OnAddUserFlagUnsafe(database, userFlag, techUser, comments);
                    }).ToList();

                    // Save Chunk Items to Database
                    database.SaveChanges();

                    return chunkResults;
                }).ToList();

                status.SetFinishedMessage($"{addUsers.Count} Users/s Added; {removeAssignments.Count} User/s Removed; {users.Count - addUsers.Count} User/s Skipped");

                return addedUserAssignments;
            }
            else
            {
                status.SetFinishedMessage("No changes found");
                return Enumerable.Empty<UserFlagAssignment>();
            }
        }
        #endregion

        public static string RandomUnusedIcon()
        {
            return UIHelpers.RandomIcon(cache.GetUserFlags().Select(f => f.flag.Icon));
        }
        public static string RandomUnusedThemeColour()
        {
            return UIHelpers.RandomThemeColour(cache.GetUserFlags().Select(f => f.flag.IconColour));
        }
    }
}
