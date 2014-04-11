using Disco.Models.Repository;
using System;
using System.Collections.Generic;

namespace Disco.Models.Interop.ActiveDirectory
{
    public class ActiveDirectoryMachineAccount : IActiveDirectoryObject
    {

        public string Domain { get; set; }
        public string SamAccountName { get; set; }

        public string SecurityIdentifier { get; set; }
        public string DistinguishedName { get; set; }
        public string Path { get; set; }

        public string Name { get; set; }
        public string DisplayName { get { return this.Name; } }
        public string DnsName { get; set; }
        public Guid NetbootGUID { get; set; }

        public bool IsCriticalSystemObject { get; set; }
        public Dictionary<string, object[]> LoadedProperties { get; set; }

        public User ToRepositoryUser()
        {
            return new User
            {
                UserId = this.Domain + "\\" + this.SamAccountName,
                DisplayName = this.Name
            };
        }

        public string NetBiosId { get { return string.Format(@"{0}\{1}", Domain, SamAccountName); } }
    }
}
