using Disco.Models.Repository;
using Disco.Services.Interop.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services
{
    public static class UserExtensions
    {
        public static bool IsInPrimaryDomain(this User u)
        {
            return u.Domain.Equals(Disco.Services.Interop.ActiveDirectory.ActiveDirectory.PrimaryDomain.NetBiosName, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string FriendlyId(this User u)
        {
            return FriendlyUserId(u.UserId);
        }

        public static string FriendlyUserId(string UserId)
        {
            var splitUserId = SplitUserId(UserId);
            
            if (splitUserId.Item1 != null && splitUserId.Item1.Equals(ActiveDirectory.PrimaryDomain.NetBiosName, StringComparison.InvariantCultureIgnoreCase))
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
