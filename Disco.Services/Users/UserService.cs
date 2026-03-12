using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Authorization.Roles;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace Disco.Services.Users
{
    public static class UserService
    {
        private const string _cacheHttpRequestKey = "Disco_CurrentUserToken";

        public static void Initialize(DiscoDataContext Database)
        {
            RoleCache.Initialize(Database);
        }

        public static string CurrentUserId
        {
            get
            {
                string userId;

                // Check for ASP.NET
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Request.IsAuthenticated)
                        userId = HttpContext.Current.User.Identity.Name;
                    else
                        return null;
                }
                else
                {
                    // User default User
                    userId = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                }

                return userId;
            }
        }

        private static Cache.CacheRecord? CurrentUserRecord
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    if (!HttpContext.Current.Request.IsAuthenticated)
                        return null; // Not Authenticated

                    if (HttpContext.Current.Items.Contains(_cacheHttpRequestKey))
                        return (Cache.CacheRecord)HttpContext.Current.Items[_cacheHttpRequestKey];
                }

                var userId = CurrentUserId;
                if (userId == null)
                    return null;

                if (!Cache.TryGet(userId, out var record))
                    return null;
                else
                {
                    if (HttpContext.Current?.Request.IsAuthenticated ?? false)
                        HttpContext.Current.Items[_cacheHttpRequestKey] = record;
                    return record;
                }

            }
        }

        public static User CurrentUser
        {
            get
            {
                var record = CurrentUserRecord;

                if (record.HasValue)
                    return record.Value.User;
                else
                    return null;
            }
        }

        public static AuthorizationToken CurrentAuthorization
        {
            get
            {
                var record = CurrentUserRecord;

                if (record.HasValue)
                    return record.Value.AuthToken;
                else
                    return null;
            }
        }

        public static User GetUser(string UserId)
        {
            if (!Cache.TryGet(UserId, out var record))
                return null;
            return record.User;
        }
        public static User GetUser(string UserId, DiscoDataContext Database)
        {
            if (!Cache.TryGet(UserId, Database, out var record))
                return null;
            return record.User;
        }
        public static User GetUser(string UserId, DiscoDataContext Database, bool ForceRefresh)
        {
            if (!Cache.TryGet(UserId, Database, ForceRefresh, out var record))
                return null;
            return record.User;
        }

        public static bool TryGetUser(string UserId, DiscoDataContext Database, bool ForceRefresh, out User User)
        {
            if (!Cache.TryGet(UserId, Database, ForceRefresh, out var record))
            {
                User = null;
                return false;
            }
            User = record.User;
            return true;
        }

        public static bool TryGetUserByUserPrincipalName(string userPrincipalName, DiscoDataContext db, bool forceRefresh, out User user)
        {
            if (!Cache.TryGetByUserPrincipalName(userPrincipalName, db, forceRefresh, out var record))
            {
                user = null;
                return false;
            }
            user = record.User;
            return true;
        }

        public static AuthorizationToken GetAuthorization(string UserId)
        {
            if (!Cache.TryGet(UserId, out var record))
                return null;
            return record.AuthToken;
        }
        public static AuthorizationToken GetAuthorization(string UserId, DiscoDataContext Database)
        {
            if (!Cache.TryGet(UserId, Database, out var record))
                return null;
            return record.AuthToken;
        }
        public static AuthorizationToken GetAuthorization(string UserId, DiscoDataContext Database, bool ForceRefresh)
        {
            if (!Cache.TryGet(UserId, Database, ForceRefresh, out var record))
                return null;
            return record.AuthToken;
        }

        public static bool InvalidateCachedUser(string UserId)
        {
            return Cache.InvalidateRecord(UserId);
        }

        public static int CreateAuthorizationRole(DiscoDataContext db, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("Role");

            var role = new AuthorizationRole()
            {
                Name = name,
                ClaimsJson = JsonConvert.SerializeObject(new RoleClaims()),
            };
            db.AuthorizationRoles.Add(role);
            db.SaveChanges();

            AuthorizationLog.LogRoleCreated(role, CurrentUserId);

            // Add to Cache
            RoleCache.AddOrUpdateRole(role);

            // Flush User Cache
            Cache.FlushCache();

            return role.Id;
        }

        public static void DeleteAuthorizationRole(DiscoDataContext db, AuthorizationRole role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            db.AuthorizationRoles.Remove(role);
            db.SaveChanges();

            AuthorizationLog.LogRoleDeleted(role, CurrentUserId);

            // Remove from Role Cache
            RoleCache.RemoveRole(role);

            // Flush User Cache
            Cache.FlushCache();
        }

        public static void UpdateAuthorizationRole(DiscoDataContext db, AuthorizationRole role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            db.SaveChanges();

            // Update Role Cache
            RoleCache.AddOrUpdateRole(role);

            // Flush User Cache
            Cache.FlushCache();
        }

        public static string GetAuthorizationRoleName(int roleId)
        {
            var role = RoleCache.GetRoleToken(roleId);
            if (role == null)
                return "Unknown authorization role";
            return role.Role.Name;
        }

        public static IEnumerable<string> AdministratorSubjectIds
        {
            get
            {
                return RoleCache.AdministratorSubjectIds;
            }
        }

        public static void UpdateAdministratorSubjectIds(DiscoDataContext db, IEnumerable<string> subjectIds)
        {
            // Update Database & In-Memory State
            RoleCache.UpdateAdministratorSubjectIds(db, subjectIds);

            // Flush User Cache
            Cache.FlushCache();
        }

        internal static List<User> SearchUsers(DiscoDataContext db, string term, bool persistResults, int? limitCount = ActiveDirectory.DefaultSearchResultLimit)
        {
            var adImportedUsers = ActiveDirectory.SearchADUserAccounts(term, Quick: true, ResultLimit: limitCount).Select(adU => adU.ToRepositoryUser()).ToList();

            if (persistResults)
            {
                foreach (var adU in adImportedUsers)
                {
                    var existingUser = db.Users.Find(adU.UserId);
                    if (existingUser != null)
                        existingUser.UpdateSelf(adU);
                    else
                        db.Users.Add(adU);
                    db.SaveChanges();
                    InvalidateCachedUser(adU.UserId);
                }
            }

            return adImportedUsers;
        }

        internal static bool TryImportUser(DiscoDataContext Database, string UserId, out User User, out AuthorizationToken AuthorizationToken)
        {
            User = null;
            AuthorizationToken = null;

            if (string.IsNullOrEmpty(UserId))
                return false;

            if (UserId.EndsWith("$"))
            {
                // Machine Account
                var adAccount = ActiveDirectory.RetrieveADMachineAccount(UserId);

                if (adAccount == null)
                    return false;

                User = adAccount.ToRepositoryUser();
                AuthorizationToken = AuthorizationToken.BuildComputerAccountToken(User);

                return true;
            }
            else
            {
                // User Account

                ADUserAccount adAccount;
                try
                {
                    adAccount = ActiveDirectory.RetrieveADUserAccount(UserId);

                    if (adAccount == null)
                        return false;
                }
                catch (COMException ex)
                {
                    // If "Server is not operational" then Try Cache
                    if (ex.ErrorCode == -2147016646)
                        SystemLog.LogException("Server is not operational; Primary Domain Controller Down?", ex);

                    return false;
                }
                catch (ActiveDirectoryOperationException ex)
                {
                    // Try From Cache...
                    SystemLog.LogException("Primary Domain Controller Down?", ex);
                    return false;
                }

                return TryImportUser(Database, adAccount, out User, out AuthorizationToken);
            }
        }

        internal static bool TryImportUser(DiscoDataContext database, ADUserAccount adUserAccount, out User user, out AuthorizationToken authorizationToken)
        {
            user = adUserAccount.ToRepositoryUser();

            // Update Repository
            User existingUser = database.Users.Find(user.UserId);
            if (existingUser == null)
            {
                database.Users.Add(user);
                database.SaveChanges();
            }
            else
            {
                if (existingUser.UpdateSelf(user))
                {
                    database.SaveChanges();
                }
                user = existingUser;
            }

            authorizationToken = AuthorizationToken.BuildToken(user, adUserAccount.Groups.Select(g => g.Id));

            return true;
        }
    }
}
