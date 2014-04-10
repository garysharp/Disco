using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Interop.ActiveDirectory
{
    public class ActiveDirectoryDomain
    {
        public string DnsName { get; private set; }
        public string NetBiosName { get; private set; }
        public string DistinguishedName { get; private set; }

        public List<string> SearchContainers { get; private set; }

        public ActiveDirectoryDomain(string DnsName, string NetBiosName, string DistinguishedName, List<string> SearchContainers)
        {
            this.DnsName = DnsName;
            this.NetBiosName = NetBiosName;
            this.DistinguishedName = DistinguishedName;
            this.SearchContainers = SearchContainers;
        }

        public void UpdateSearchContainers(IEnumerable<string> Containers)
        {
            this.SearchContainers = Containers.ToList();
        }
    }
}