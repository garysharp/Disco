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
            Authorization.Roles.RoleCache.Initialize(Database);
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

        private static Tuple<User, AuthorizationToken, DateTime> CurrentUserToken
        {
            get
            {
                Tuple<User, AuthorizationToken, DateTime> token = null;

                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Request.IsAuthenticated)
                        token = (Tuple<User, AuthorizationToken, DateTime>)HttpContext.Current.Items[_cacheHttpRequestKey];
                    else
                        return null; // Not Authenticated
                }

                if (token == null)
                {
                    var userId = CurrentUserId;

                    if (userId != null)
                    {
                        token = Cache.Get(userId);

                        if (HttpContext.Current != null && HttpContext.Current.Request.IsAuthenticated)
                            HttpContext.Current.Items[_cacheHttpRequestKey] = token;
                    }
                }

                return token;
            }
        }
        public static User CurrentUser
        {
            get
            {
                var token = CurrentUserToken;

                if (token == null)
                    return null;
                else
                    return token.Item1;
            }
        }
        public static AuthorizationToken CurrentAuthorization
        {
            get
            {
                var token = CurrentUserToken;

                if (token == null)
                    return null;
                else
                    return token.Item2;
            }
        }

        public static User GetUser(string UserId)
        {
            return Cache.GetUser(UserId);
        }
        public static User GetUser(string UserId, DiscoDataContext Database)
        {
            return Cache.GetUser(UserId, Database);
        }
        public static User GetUser(string UserId, DiscoDataContext Database, bool ForceRefresh)
        {
            return Cache.GetUser(UserId, Database, ForceRefresh);
        }

        public static bool TryGetUser(string UserId, DiscoDataContext Database, bool ForceRefresh, out User User)
        {
            return Cache.TryGetUser(UserId, Database, ForceRefresh, out User);
        }

        public static AuthorizationToken GetAuthorization(string UserId)
        {
            return Cache.GetAuthorization(UserId);
        }
        public static AuthorizationToken GetAuthorization(string UserId, DiscoDataContext Database)
        {
            return Cache.GetAuthorization(UserId, Database);
        }
        public static AuthorizationToken GetAuthorization(string UserId, DiscoDataContext Database, bool ForceRefresh)
        {
            return Cache.GetAuthorization(UserId, Database, ForceRefresh);
        }

        public static bool InvalidateCachedUser(string UserId)
        {
            return Cache.InvalidateRecord(UserId);
        }

        public static int CreateAuthorizationRole(DiscoDataContext Database, AuthorizationRole Role)
        {
            if (Role == null)
                throw new ArgumentNullException("Role");

            if (string.IsNullOrWhiteSpace(Role.ClaimsJson))
                Role.ClaimsJson = JsonConvert.SerializeObject(new RoleClaims());

            Database.AuthorizationRoles.Add(Role);
            Database.SaveChanges();

            AuthorizationLog.LogRoleCreated(Role, CurrentUserId);

            // Add to Cache
            RoleCache.AddRole(Role);

            // Flush User Cache
            Cache.FlushCache();

            return Role.Id;
        }
        public static void DeleteAuthorizationRole(DiscoDataContext Database, AuthorizationRole Role)
        {
            if (Role == null)
                throw new ArgumentNullException("Role");

            Database.AuthorizationRoles.Remove(Role);
            Database.SaveChanges();

            AuthorizationLog.LogRoleDeleted(Role, CurrentUserId);

            // Remove from Role Cache
            RoleCache.RemoveRole(Role);

            // Flush User Cache
            Cache.FlushCache();
        }
        public static void UpdateAuthorizationRole(DiscoDataContext Database, AuthorizationRole Role)
        {
            if (Role == null)
                throw new ArgumentNullException("Role");
            if (Database == null)
                throw new ArgumentNullException("Database");

            Database.SaveChanges();

            // Update Role Cache
            RoleCache.UpdateRole(Role);

            // Flush User Cache
            Cache.FlushCache();
        }

        public static IEnumerable<string> AdministratorSubjectIds
        {
            get
            {
                return RoleCache.AdministratorSubjectIds;
            }
        }
        public static void UpdateAdministratorSubjectIds(DiscoDataContext Database, IEnumerable<string> SubjectIds)
        {
            // Update Database & In-Memory State
            RoleCache.UpdateAdministratorSubjectIds(Database, SubjectIds);

            // Flush User Cache
            Cache.FlushCache();
        }

        internal static List<User> SearchUsers(DiscoDataContext Database, string Term, bool PersistResults, int? LimitCount = ActiveDirectory.DefaultSearchResultLimit)
        {
            var adImportedUsers = ActiveDirectory.SearchADUserAccounts(Term, Quick: true, ResultLimit: LimitCount).Select(adU => adU.ToRepositoryUser()).ToList();

            if (PersistResults)
            {
                foreach (var adU in adImportedUsers)
                {
                    var existingUser = Database.Users.Find(adU.UserId);
                    if (existingUser != null)
                        existingUser.UpdateSelf(adU);
                    else
                        Database.Users.Add(adU);
                    Database.SaveChanges();
                    UserService.InvalidateCachedUser(adU.UserId);
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

                User = adAccount.ToRepositoryUser();

                // Update Repository
                User existingUser = Database.Users.Find(User.UserId);
                if (existingUser == null)
                {
                    Database.Users.Add(User);
                    Database.SaveChanges();
                }
                else
                {
                    if (existingUser.UpdateSelf(User))
                    {
                        Database.SaveChanges();
                    }
                    User = existingUser;
                }

                AuthorizationToken = AuthorizationToken.BuildToken(User, adAccount.Groups.Select(g => g.Id));

                return true;
            }
        }

        internal static Tuple<User, AuthorizationToken> ImportUser(DiscoDataContext Database, string UserId)
        {
            if (string.IsNullOrEmpty(UserId))
                throw new ArgumentNullException(nameof(UserId));

            if (TryImportUser(Database, UserId, out var user, out var authorization))
            {
                return Tuple.Create(user, authorization);
            }
            else
            {
                throw new ArgumentException($"Unable to import Active Directory user '{UserId}'", nameof(UserId));
            }
        }
    }
}
