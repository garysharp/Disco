using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;

namespace Disco.Models.Interop.ActiveDirectory
{
    public class ActiveDirectoryUserAccount : IActiveDirectoryObject
    {
        public string DisplayName { get; set; }
        public string DistinguishedName { get; set; }
        public string Domain { get; set; }
        public string Email { get; set; }
        public string GivenName { get; set; }
        public List<string> Groups { get; set; }
        public string Name { get; set; }
        public string SecurityIdentifier { get; set; }
        public string Path { get; set; }
        public string Phone { get; set; }
        public string SamAccountName { get; set; }
        public string Surname { get; set; }
        public Dictionary<string, object[]> LoadedProperties { get; set; }

        public User ToRepositoryUser()
        {
            return new User
            {
                Id = this.SamAccountName,
                DisplayName = this.DisplayName,
                Surname = this.Surname,
                GivenName = this.GivenName,
                EmailAddress = this.Email,
                PhoneNumber = this.Phone,
            };
        }

    }
}
