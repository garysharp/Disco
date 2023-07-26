using Disco.Data.Repository;
using Disco.Models.Services.Authorization;
using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using Disco.Services.Interop.ActiveDirectory;
using Newtonsoft.Json;

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
            MigrateAuthorizationRoles(Database);

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
            var configuredSubjectIds = Database.DiscoConfiguration.Administrators.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => ActiveDirectory.ParseDomainAccountId(s));

            return RequiredAdministratorSubjectIds
                .Concat(configuredSubjectIds)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s);
        }
        public static IEnumerable<string> RequiredAdministratorSubjectIds
        {
            get
            {
                return _RequiredAdministratorSubjectIds.Select(s => ActiveDirectory.ParseDomainAccountId(s));
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
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s);

            var subjectIdsString = string.Join(",", SubjectIds);

            // Update Database
            Database.DiscoConfiguration.Administrators = subjectIdsString;
            Database.SaveChanges();

            // Update State
            _AdministratorToken.SubjectIds = SubjectIds.ToList();
            _AdministratorToken.SubjectIdHashes = new HashSet<string>(SubjectIds, StringComparer.OrdinalIgnoreCase);
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
            return _Cache.FirstOrDefault(t => t.SubjectIdHashes.Contains(SecurityGroup));
        }
        internal static List<IRoleToken> GetRoleTokens(IEnumerable<string> SecurityGroup)
        {
            return _Cache.Where(t => SecurityGroup.Any(sg => t.SubjectIdHashes.Contains(sg))).Cast<IRoleToken>().ToList();
        }
        internal static List<IRoleToken> GetRoleTokens(IEnumerable<string> SecurityGroup, User User)
        {
            var subjectIds = SecurityGroup.Concat(new string[] { User.UserId });

            return _Cache.Where(t => subjectIds.Any(sg => t.SubjectIdHashes.Contains(sg))).Cast<IRoleToken>().ToList();
        }

        /// <summary>
        /// Migrates authorization role claims to conform with changes to Disco ICT since the last release.
        /// Claims are only added when the meaning of an existing claim has changed (or expanded/contracted) to improve the migration experience.
        /// </summary>
        private static void MigrateAuthorizationRoles(DiscoDataContext Database)
        {
            // Determine roles which need migration from DBv11 -> DBv14
            var affectedRoles_DBv14 = Database.AuthorizationRoles.Where(r => !r.ClaimsJson.Contains("MyJobs")).ToList();
            
            // Determine roles which need migration from DBv14 -> DBv15
            var affectedRoles_DBv15 = Database.AuthorizationRoles.Where(r => !r.ClaimsJson.Contains("RepairProviderDetails")).ToList();

            if (affectedRoles_DBv14.Count > 0)
            {
                foreach (var role in affectedRoles_DBv14)
                {
                    var claims = JsonConvert.DeserializeObject<RoleClaims>(role.ClaimsJson);

                    // MyJobs replaces 'AwaitingTechnicianAction' jobs on the Job page.
                    if (claims.Job.Lists.AwaitingTechnicianAction)
                    {
                        claims.Job.Lists.MyJobs = true;
                        claims.Job.Lists.MyJobsOrphaned = true;
                    }
                    // Stale Jobs expands on Long Running Jobs (and replaces it on the Job page)
                    if (claims.Job.Lists.LongRunningJobs)
                    {
                        claims.Job.Lists.StaleJobs = true;
                    }
                    // Greater control to create jobs was added, this adds claims to keep the behaviour the same for existing roles
                    if (claims.Job.Actions.Create)
                    {
                        claims.Job.Types.CreateHMisc = true;
                        claims.Job.Types.CreateHNWar = true;
                        claims.Job.Types.CreateHWar = true;
                        claims.Job.Types.CreateSApp = true;
                        claims.Job.Types.CreateSOS = true;
                        claims.Job.Types.CreateSImg = true;
                        claims.Job.Types.CreateUMgmt = true;
                    }
                    // A claim was added to control whether Current User Assignments could be shown (independently of User Assignment History)
                    if (claims.User.ShowAssignmentHistory)
                    {
                        claims.User.ShowAssignments = true;
                    }
                    // A claim was added to control whether User personal details could be shown
                    if (claims.User.Show)
                    {
                        claims.User.ShowDetails = true;
                    }

                    role.ClaimsJson = Newtonsoft.Json.JsonConvert.SerializeObject(claims);
                }

                Database.SaveChanges();
            }

            if (affectedRoles_DBv15.Count > 0)
            {
                foreach (var role in affectedRoles_DBv15)
                {
                    var claims = JsonConvert.DeserializeObject<RoleClaims>(role.ClaimsJson);

                    // If the user previously had the ability to view warranty provider details, they probably should be able to view repair provider details (new feature).
                    if (claims.Job.Properties.WarrantyProperties.ProviderDetails)
                    {
                        claims.Job.Properties.NonWarrantyProperties.RepairProviderDetails = true;
                    }

                    role.ClaimsJson = Newtonsoft.Json.JsonConvert.SerializeObject(claims);
                }

                Database.SaveChanges();
            }
        }
    }
}
