using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;

namespace Disco.Models.Interop.ActiveDirectory
{
    public class ActiveDirectoryMachineAccount : IActiveDirectoryObject
    {
        public string DistinguishedName { get; set; }
        public string DnsName { get; set; }
        public string Domain { get; set; }
        public string Name { get; set; }
        public Guid NetbootGUID { get; set; }
        public string SecurityIdentifier { get; set; }
        public string Path { get; set; }
        public string SamAccountName { get; set; }
        public bool IsCriticalSystemObject { get; set; }
        public Dictionary<string, object[]> LoadedProperties { get; set; }

        public User ToRepositoryUser()
        {
            return new User
            {
                Id = this.SamAccountName,
                DisplayName = this.Name
            };
        }

    }
}
