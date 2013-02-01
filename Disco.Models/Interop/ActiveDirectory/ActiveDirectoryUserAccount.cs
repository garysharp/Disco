using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;

namespace Disco.Models.Interop.ActiveDirectory
{
    public class ActiveDirectoryUserAccount
    {
        public string DisplayName { get; set; }
        public string DistinguishedName { get; set; }
        public string Domain { get; set; }
        public string Email { get; set; }
        public string GivenName { get; set; }
        public List<string> Groups { get; set; }
        public string Name { get; set; }
        public string ObjectSid { get; set; }
        public string Path { get; set; }
        public string Phone { get; set; }
        public string sAMAccountName { get; set; }
        public string Surname { get; set; }
        public string Type { get; set; }
        public Dictionary<string, object[]> LoadedProperties { get; set; }

        public User ToRepositoryUser()
        {
            return new User
            {
                Id = this.sAMAccountName,
                DisplayName = this.DisplayName,
                Surname = this.Surname,
                GivenName = this.GivenName,
                EmailAddress = this.Email,
                PhoneNumber = this.Phone,
                Type = this.Type
            };
        }

    }
}
