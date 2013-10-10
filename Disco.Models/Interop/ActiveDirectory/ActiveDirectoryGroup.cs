using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Interop.ActiveDirectory
{
    public class ActiveDirectoryGroup : IActiveDirectoryObject
    {
        public string Name { get; set; }
        public string DistinguishedName { get; set; }
        public string SamAccountName { get; set; }
        public string SecurityIdentifier { get; set; }
        public string CommonName { get; set; }

        public List<string> MemberOf { get; set; }
    }
}
