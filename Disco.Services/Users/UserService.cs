using Disco.Data.Repository;
using Disco.Models.Interop.ActiveDirectory;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Authorization.Roles;
using Disco.Services.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Disco.Services.Users
{
    public static class UserService
    {
        private const string _cacheHttpRequestKey = "Disco_CurrentUserToken";
        private static Func<string, string[], ActiveDirectoryUserAccount> _GetActiveDirectoryUserAccount;
        private static Func<string, string[], ActiveDirectoryMachineAccount> _GetActiveDirectoryMachineAccount;
        private static Func<string, List<ActiveDirectoryUserAccount>> _SearchActiveDirectoryUsers;

        public static void Initialize(DiscoDataContext Database,
            Func<string, string[], ActiveDirectoryUserAccount> GetActiveDirectoryUserAccount,
            Func<string, string[], ActiveDirectoryMachineAccount> GetActiveDirectoryMachineAccount,
            Func<string, List<ActiveDirectoryUserAccount>> SearchActiveDirectoryUsers)
        {
            _GetActiveDirectoryUserAccount = GetActiveDirectoryUserAccount;
            _GetActiveDirectoryMachineAccount = GetActiveDirectoryMachineAccount;
            _SearchActiveDirectoryUsers = SearchActiveDirectoryUsers;

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

                if (userId.Contains("\\"))
                    return userId.Substring(checked(userId.IndexOf("\\") + 1));
                else
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

        internal static List<ActiveDirectoryUserAccount> SearchUsers(string Term)
        {
            return _SearchActiveDirectoryUsers(Term);
        }

        internal static List<ActiveDirectoryUserAccount> SearchUsers(DiscoDataContext Database, string Term)
        {
            var adImportedUsers = SearchUsers(Term);
            foreach (var adU in adImportedUsers.Select(adU => adU.ToRepositoryUser()))
            {
                var existingUser = Database.Users.Find(adU.Id);
                if (existingUser != null)
                    existingUser.UpdateSelf(adU);
                else
                    Database.Users.Add(adU);
                Database.SaveChanges();
                UserService.InvalidateCachedUser(adU.Id);
            }
            return adImportedUsers;
        }

        internal static Tuple<User, AuthorizationToken> ImportUser(DiscoDataContext Database, string UserId)
        {
            if (_GetActiveDirectoryUserAccount == null)
                throw new InvalidOperationException("UserServer has not been Initialized");
            if (string.IsNullOrEmpty(UserId))
                throw new ArgumentNullException("UserId is required", "UserId");

            if (UserId.EndsWith("$"))
            {
                // Machine Account
                var adAccount = _GetActiveDirectoryMachineAccount(UserId, null);

                if (adAccount == null)
                    return null;

                var user = adAccount.ToRepositoryUser();
                var token = AuthorizationToken.BuildComputerAccountToken(user);

                return new Tuple<User, AuthorizationToken>(user, token);
            }
            else
            {
                // User Account

                ActiveDirectoryUserAccount adAccount;
                try
                {
                    adAccount = _GetActiveDirectoryUserAccount(UserId, null);

                    if (adAccount == null)
                        throw new ArgumentException(string.Format("Invalid Username: '{0}'; User not found in Active Directory", UserId), "Username");
                }
                catch (COMException ex)
                {
                    // If "Server is not operational" then Try Cache
                    if (ex.ErrorCode == -2147016646)
                        SystemLog.LogException("Server is not operational; Primary Domain Controller Down?", ex);

                    throw ex;
                }
                catch (ActiveDirectoryOperationException ex)
                {
                    // Try From Cache...
                    SystemLog.LogException("Primary Domain Controller Down?", ex);
                    throw ex;
                }

                var user = adAccount.ToRepositoryUser();

                // Update Repository
                User existingUser = Database.Users.Find(user.Id);
                if (existingUser == null)
                    Database.Users.Add(user);
                else
                {
                    existingUser.UpdateSelf(user);
                    user = existingUser;
                }
                Database.SaveChanges();

                var token = AuthorizationToken.BuildToken(user, adAccount.Groups);

                return new Tuple<User, AuthorizationToken>(user, token);
            }
        }
    }
}
