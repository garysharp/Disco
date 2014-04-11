using Disco.Data.Repository;
using Disco.Models.Services.Authorization;
using Disco.Models.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization.Roles
{
    internal static class RoleCache
    {
        internal const int AdministratorsTokenId = -1;
        internal const int ComputerAccountTokenId = -200;
        internal const string ClaimsJsonEmpty = "null";
        internal static readonly string[] _RequiredAdministratorSubjectIds = new string[] { "Domain Admins" };

        private static List<RoleToken> _Cache;
        private static RoleToken _AdministratorToken;

        internal static void Initialize(DiscoDataContext Database)
        {
            _Cache = Database.AuthorizationRoles.ToList().Select(ar => RoleToken.FromAuthorizationRole(ar)).ToList();

            // Add System Roles
            AddSystemRoles(Database);
        }

        private static void AddSystemRoles(DiscoDataContext Database)
        {
            // Disco Administrators
            _AdministratorToken = RoleToken.FromAuthorizationRole(new AuthorizationRole()
            {
                Id = AdministratorsTokenId,
                Name = "Disco Administrators",
                SubjectIds = string.Join(",", GenerateAdministratorSubjectIds(Database))
            }, Claims.AdministratorClaims());
            _Cache.Add(_AdministratorToken);

            // Computer Accounts
            _Cache.Add(RoleToken.FromAuthorizationRole(new AuthorizationRole()
            {
                Id = ComputerAccountTokenId,
                Name = "Domain Computer Account"
            }, Claims.ComputerAccountClaims()));
        }

        private static IEnumerable<string> GenerateAdministratorSubjectIds(DiscoDataContext Database)
        {
            var domainNetBiosName = Interop.ActiveDirectory.ActiveDirectory.PrimaryDomain.NetBiosName;
            var configuredSubjectIds = Database.DiscoConfiguration.Administrators.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Contains(@"\") ? s : string.Format(@"{0}\{1}", domainNetBiosName, s));

            return RequiredAdministratorSubjectIds
                .Concat(configuredSubjectIds)
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .OrderBy(s => s);
        }
        public static IEnumerable<string> RequiredAdministratorSubjectIds
        {
            get
            {
                var domainNetBiosName = Interop.ActiveDirectory.ActiveDirectory.PrimaryDomain.NetBiosName;
                return _RequiredAdministratorSubjectIds.Select(s => string.Format(@"{0}\{1}", domainNetBiosName, s));
            }
        }
        public static IEnumerable<string> AdministratorSubjectIds
        {
            get
            {
                return _AdministratorToken.SubjectIds.ToList();
            }
        }

        public static void UpdateAdministratorSubjectIds(DiscoDataContext Database, IEnumerable<string> SubjectIds)
        {
            // Clean
            SubjectIds = SubjectIds
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Concat(RequiredAdministratorSubjectIds)
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .OrderBy(s => s);

            var subjectIdsString = string.Join(",", SubjectIds);

            // Update Database
            Database.DiscoConfiguration.Administrators = subjectIdsString;
            Database.SaveChanges();

            // Update State
            _AdministratorToken.SubjectIds = SubjectIds.ToList();
            _AdministratorToken.SubjectIdHashes = new HashSet<string>(SubjectIds.Select(i => i.ToLower()));
        }

        /// <summary>
        /// Create a clone of an Authorization Role
        /// <para>Creates immutable clones to avoid side-effects</para>
        /// </summary>
        /// <param name="TemplateRole">Authorization Role to Clone</param>
        /// <returns>A copy of the Authorization Role</returns>
        private static AuthorizationRole CloneAuthoriationRole(AuthorizationRole TemplateRole)
        {
            return new AuthorizationRole()
            {
                Id = TemplateRole.Id,
                Name = TemplateRole.Name,
                ClaimsJson = TemplateRole.ClaimsJson,
                SubjectIds = TemplateRole.SubjectIds
            };
        }

        internal static RoleToken AddRole(AuthorizationRole Role)
        {
            var token = RoleToken.FromAuthorizationRole(CloneAuthoriationRole(Role));
            _Cache.Add(token);
            return token;
        }

        internal static void RemoveRole(AuthorizationRole Role)
        {
            var token = GetRoleToken(Role.Id);
            if (token != null)
                _Cache.Remove(token);
        }

        internal static RoleToken UpdateRole(AuthorizationRole Role)
        {
            RemoveRole(Role);
            return AddRole(Role);
        }

        internal static RoleToken GetRoleToken(int Id)
        {
            return _Cache.FirstOrDefault(t => t.Role.Id == Id);
        }
        internal static RoleToken GetRoleToken(string SecurityGroup)
        {
            return _Cache.FirstOrDefault(t => t.SubjectIdHashes.Contains(SecurityGroup.ToLower()));
        }
        internal static List<IRoleToken> GetRoleTokens(IEnumerable<string> SecurityGroup)
        {
            var securityGroups = SecurityGroup.Select(sg => sg.ToLower());

            return _Cache.Where(t => securityGroups.Any(sg => t.SubjectIdHashes.Contains(sg))).Cast<IRoleToken>().ToList();
        }
        internal static List<IRoleToken> GetRoleTokens(IEnumerable<string> SecurityGroup, User User)
        {
            var subjectIds = (new string[] { User.UserId }).Concat(SecurityGroup).Select(sg => sg.ToLower());

            return _Cache.Where(t => subjectIds.Any(sg => t.SubjectIdHashes.Contains(sg))).Cast<IRoleToken>().ToList();
        }
    }
}
