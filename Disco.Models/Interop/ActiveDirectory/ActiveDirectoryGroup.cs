using System.Collections.Generic;

namespace Disco.Models.Interop.ActiveDirectory
{
    public class ActiveDirectoryGroup : IActiveDirectoryObject
    {
        public string Domain { get; set; }
        public string SamAccountName { get; set; }

        public string DistinguishedName { get; set; }
        public string SecurityIdentifier { get; set; }
        public string CommonName { get; set; }

        public string Name { get; set; }
        public string DisplayName { get { return this.Name; } }

        public List<string> MemberOf { get; set; }

        public string NetBiosId { get { return string.Format(@"{0}\{1}", Domain, SamAccountName); } }
    }
}
