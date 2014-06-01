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
            return FriendlyUserId(u.UserId);
        }

        public static string FriendlyUserId(string UserId)
        {
            var splitUserId = SplitUserId(UserId);

            if (splitUserId.Item1 != null && splitUserId.Item1.Equals(ActiveDirectory.Context.PrimaryDomain.NetBiosName, StringComparison.OrdinalIgnoreCase))
                return splitUserId.Item2;
            else
                return UserId;
        }

        public static Tuple<string, string> SplitUserId(string UserId)
        {
            var slashIndex = UserId.IndexOf('\\');
            if (slashIndex < 0)
                return Tuple.Create<string, string>(null, UserId);
            else
                return Tuple.Create(UserId.Substring(0, slashIndex), UserId.Substring(slashIndex + 1));
        }
    }
}
