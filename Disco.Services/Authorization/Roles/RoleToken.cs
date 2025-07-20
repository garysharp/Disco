using Disco.Models.Repository;
using Disco.Models.Services.Authorization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Authorization.Roles
{
    public class RoleToken : IRoleToken
    {
        public AuthorizationRole Role { get; internal set; }
        internal HashSet<string> SubjectIdHashes { get; set; }
        public List<string> SubjectIds { get; internal set; }
        public RoleClaims Claims { get; internal set; }

        public static RoleToken FromAuthorizationRole(AuthorizationRole Role)
        {
            var claims = JsonConvert.DeserializeObject<RoleClaims>(Role.ClaimsJson);

            return FromAuthorizationRole(Role, claims);
        }

        internal static RoleToken FromAuthorizationRole(AuthorizationRole Role, RoleClaims Claims)
        {
            string[] sg = (Role.SubjectIds == null ? new string[0] : Role.SubjectIds.Split(',').ToArray());

            return new RoleToken()
            {
                Role = Role,
                SubjectIdHashes = new HashSet<string>(sg, StringComparer.OrdinalIgnoreCase),
                SubjectIds = sg.ToList(),
                Claims = Claims
            };
        }
    }
}
