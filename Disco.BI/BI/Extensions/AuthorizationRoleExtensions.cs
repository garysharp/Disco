using Disco.Data.Repository;
using Disco.Models.Services.Authorization;
using Disco.Models.Repository;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.BI.Extensions
{
    public static class AuthorizationRoleExtensions
    {

        public static void Delete(this IRoleToken roleToken, DiscoDataContext Database)
        {
            var role = Database.AuthorizationRoles.Find(roleToken.Role.Id);            
            UserService.DeleteAuthorizationRole(Database, roleToken.Role);
        }

        public static void Delete(this AuthorizationRole role, DiscoDataContext Database)
        {
            UserService.DeleteAuthorizationRole(Database, role);
        }

    }
}
