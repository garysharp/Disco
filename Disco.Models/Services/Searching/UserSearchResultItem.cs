using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Services.Searching
{
    public class UserSearchResultItem : ISearchResultItem
    {
        private const string type = "User";

        public string Id { get; set; }
        public string Type { get { return type; } }
        public string Description { get { return string.Format("{0} ({1})", this.DisplayName, this.Id); } }
        public string ScoreValue { get { return string.Format("{0} {1}", this.Id, this.DisplayName); } }

        public int AssignedDevicesCount { get; set; }
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public int JobCount { get; set; }
        public string Surname { get; set; }
    }
}
