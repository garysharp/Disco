using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Extensions;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Users.UserFlags
{
    public static class UserFlagService
    {
        private static Cache _cache;

        public static void Initialize(DiscoDataContext Database)
        {
            _cache = new Cache(Database);
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
                IconColour = UserFlag.IconColour
            };

            Database.UserFlags.Add(flag);
            Database.SaveChanges();

            return _cache.Update(flag);
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

            return _cache.Update(UserFlag);
        }
        public static void DeleteUserFlag(DiscoDataContext Database, int UserFlagId, IScheduledTaskStatus Status)
        {
            UserFlag flag = Database.UserFlags.Find(UserFlagId);

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
