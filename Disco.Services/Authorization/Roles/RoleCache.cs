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
        internal const string AdministratorsTokenSubjectIds = "Domain Admins,Disco Admins";
        internal const string ClaimsJsonEmpty = "null";

        private static List<RoleToken> _Cache;

        internal static void Initialize(DiscoDataContext Database)
        {
            _Cache = Database.AuthorizationRoles.ToList().Select(ar => RoleToken.FromAuthorizationRole(ar)).ToList();

            // Add System Roles
            AddSystemRoles();
        }

        private static void AddSystemRoles()
        {
            // Disco Administrators
            _Cache.Add(RoleToken.FromAuthorizationRole(new AuthorizationRole()
            {
                Id = AdministratorsTokenId,
                Name = "Disco Administrators",
                SubjectIds = AdministratorsTokenSubjectIds
            }, Claims.AdministratorClaims()));

            // Computer Accounts
            _Cache.Add(RoleToken.FromAuthorizationRole(new AuthorizationRole()
            {
                Id = ComputerAccountTokenId,
                Name = "Domain Computer Account"
            }, Claims.ComputerAccountClaims()));
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
            var subjectIds = (new string[] { User.Id }).Concat(SecurityGroup).Select(sg => sg.ToLower());

            return _Cache.Where(t => subjectIds.Any(sg => t.SubjectIdHashes.Contains(sg))).Cast<IRoleToken>().ToList();
        }
    }
}
