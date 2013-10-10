using Disco.Models.Authorization;
using Disco.Models.Repository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization.Roles
{
    public class RoleToken : IRoleToken
    {
        public AuthorizationRole Role { get; set; }
        internal HashSet<string> SubjectIdHashes { get; set; }
        public List<string> SubjectIds { get; set; }
        public RoleClaims Claims { get; set; }

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
                SubjectIdHashes = new HashSet<string>(sg.Select(i => i.ToLower())),
                SubjectIds = sg.ToList(),
                Claims = Claims
            };
        }
    }
}
