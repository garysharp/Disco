using System;
using System.Collections;
using System.DirectoryServices;

namespace Disco.Client.Interop
{
    public static class LocalAuthentication
    {

        public static bool AddLocalGroupMembership(string GroupName, string UserSID, string Username, string UserDomain)
        {

            using (DirectoryEntry group = new DirectoryEntry($"WinNT://./{GroupName},group"))
            {
                // Check to see if the User is already a member
                foreach (object memberRef in (IEnumerable)group.Invoke("Members"))
                {
                    using (DirectoryEntry member = new DirectoryEntry(memberRef))
                    {
                        var memberPath = member.Path;
                        if (memberPath.Equals($"WinNT://{UserDomain}/{Username}", StringComparison.OrdinalIgnoreCase) ||
                            memberPath.Equals($"WinNT://{UserSID}", StringComparison.OrdinalIgnoreCase))
                            return false;
                    }
                }
                group.Invoke("Add", $"WinNT://{UserSID}");
            }
            return true;
        }

    }
}
