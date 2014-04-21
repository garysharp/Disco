using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Client.Interop
{
    public static class LocalAuthentication
    {

        public static bool AddLocalGroupMembership(string GroupName, string UserSID, string Username, string UserDomain)
        {

            using (DirectoryEntry group = new DirectoryEntry(string.Format("WinNT://./{0},group", GroupName)))
            {
                // Check to see if the User is already a member
                foreach (object memberRef in (IEnumerable)group.Invoke("Members"))
                {
                    using (DirectoryEntry member = new DirectoryEntry(memberRef))
                    {
                        var memberPath = member.Path;
                        if (memberPath.Equals(string.Format("WinNT://{0}/{1}", UserDomain, Username), StringComparison.OrdinalIgnoreCase) ||
                            memberPath.Equals(string.Format("WinNT://{0}", UserSID), StringComparison.OrdinalIgnoreCase))
                            return false;
                    }
                }
                group.Invoke("Add", string.Format("WinNT://{0}", UserSID));
            }
            return true;
        }

        public static string CurrentUserDomain
        {
            get
            {
                return Environment.UserDomainName;
            }
        }
        public static string CurrentUserName
        {
            get
            {
                return Environment.UserName;
            }
        }

        public static string ComputerName
        {
            get
            {
                return Environment.MachineName;
            }
        }

    }
}
