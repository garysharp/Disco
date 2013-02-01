using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Models.BI.Search
{
    public class UserSearchResultItem
    {
        public int AssignedDevicesCount { get; set; }
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string Id { get; set; }
        public int JobCount { get; set; }
        public string Surname { get; set; }
    }
}
