using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Authorization;
using Disco.Services.Users;
using System;

namespace Disco.Services
{
    public static class AuthorizationRoleExtensions
    {

        public static void Delete(this IRoleToken roleToken, DiscoDataContext Database)
        {
            var role = Database.AuthorizationRoles.Find(roleToken.Role.Id)
                ?? throw new ArgumentException("Role not found", nameof(roleToken));
            UserService.DeleteAuthorizationRole(Database, role);
        }

        public static void Delete(this AuthorizationRole role, DiscoDataContext Database)
        {
            UserService.DeleteAuthorizationRole(Database, role);
        }

    }
}
