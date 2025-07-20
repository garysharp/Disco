using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Users;
using System;
using System.Data.Entity;
using System.Linq;

namespace Disco.Services.Expressions.Extensions
{
    public static class UserExt
    {
        #region Active Directory Extensions
        public static object GetActiveDirectoryObjectValue(User User, string PropertyName, int Index = 0)
        {
            var adUserAccount = User.ActiveDirectoryAccount(PropertyName);
            if (adUserAccount != null)
                return adUserAccount.GetPropertyValues<object>(PropertyName).Skip(Index).FirstOrDefault();
            else
                return null;
        }

        public static string GetActiveDirectoryStringValue(User User, string PropertyName, int Index = 0)
        {
            var objectValue = GetActiveDirectoryObjectValue(User, PropertyName, Index);
            string stringValue = objectValue as string;
            if (stringValue == null && objectValue != null)
                stringValue = objectValue.ToString();
            return stringValue;
        }

        public static int GetActiveDirectoryIntegerValue(User User, string PropertyName, int Index = 0)
        {
            var objectValue = GetActiveDirectoryObjectValue(User, PropertyName, Index);
            if (objectValue == null)
                return default;
            else
            {
                int intValue;
                try
                {
                    intValue = (int)Convert.ChangeType(objectValue, typeof(int));
                }
                catch (Exception)
                {
                    throw;
                }
                return intValue;
            }
        }
        #endregion

        #region Authorization Testing Extensions
        public static bool HasAuthorization(User User, string Claim)
        {
            var authorization = UserService.GetAuthorization(User.UserId);

            return authorization.Has(Claim);
        }

        public static bool HasAuthorizationAll(User User, params string[] Claims)
        {
            var authorization = UserService.GetAuthorization(User.UserId);

            return authorization.HasAll(Claims);
        }

        public static bool HasAuthorizationAny(User User, params string[] Claims)
        {
            var authorization = UserService.GetAuthorization(User.UserId);

            return authorization.HasAny(Claims);
        }
        #endregion

        #region Flag Extensions

        public static bool AddFlag(User user, string flagName, User techUser)
            => AddFlag(user, flagName, techUser, comments: null);

        public static bool AddFlag(User user, string flagName, User techUser, string comments)
        {
            using (var database = new DiscoDataContext())
            {
                database.Configuration.LazyLoadingEnabled = true;

                var flag = database.UserFlags.Single(f => f.Name == flagName);
                if (flag == null)
                    throw new ArgumentException("Invalid User Flag Name", nameof(flagName));

                var flagUser = database.Users.Include(u => u.UserFlagAssignments).FirstOrDefault(u => u.UserId == user.UserId);
                if (flagUser == null)
                    throw new ArgumentException("Invalid User", nameof(user));

                if (flagUser.UserFlagAssignments.Any(fa => !fa.RemovedDate.HasValue && fa.UserFlagId == flag.Id))
                    return false;

                var addingUser = database.Users.Find(techUser.UserId);
                if (addingUser == null)
                    throw new ArgumentException("Invalid Tech User", nameof(techUser));

                var userFlagAssignment = flagUser.OnAddUserFlagUnsafe(database, flag, addingUser, comments);

                database.SaveChanges();
            }

            return true;
        }

        #endregion
    }
}
