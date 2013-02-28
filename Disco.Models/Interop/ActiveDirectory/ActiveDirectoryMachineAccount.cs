using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;

namespace Disco.Models.Interop.ActiveDirectory
{
    public class ActiveDirectoryMachineAccount
    {
        public string DistinguishedName { get; set; }
        public string DnsName { get; set; }
        public string Domain { get; set; }
        public string Name { get; set; }
        public Guid NetbootGUID { get; set; }
        public string ObjectSid { get; set; }
        public string Path { get; set; }
        public string sAMAccountName { get; set; }
        public bool IsCriticalSystemObject { get; set; }
        public Dictionary<string, object[]> LoadedProperties { get; set; }

        public string ParentDistinguishedName
        {
            get
            {
                // Determine Parent
                if (!string.IsNullOrWhiteSpace(DistinguishedName))
                    return DistinguishedName.Substring(0, DistinguishedName.IndexOf(",DC=")).Substring(DistinguishedName.IndexOf(",") + 1);
                else
                    return null;
            }
        }

        public User ToRepositoryUser()
        {
            return new User
            {
                Id = this.sAMAccountName,
                Type = "Computer",
                DisplayName = this.Name
            };
        }

    }
}
