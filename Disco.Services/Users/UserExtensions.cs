using Disco.Models.Repository;
using Disco.Services.Interop.ActiveDirectory;
using System;

namespace Disco.Services
{
    public static class UserExtensions
    {
        public static bool IsInPrimaryDomain(this User u)
        {
            return u.Domain.Equals(ActiveDirectory.Context.PrimaryDomain.NetBiosName, StringComparison.OrdinalIgnoreCase);
        }

        public static string ToStringFriendly(this User u)
        {
            return string.Format("{0} ({1})", u.DisplayName, u.FriendlyId());
        }

        public static string FriendlyId(this User u)
        {
            return ActiveDirectory.FriendlyAccountId(u.UserId);
        }
    }
}
