using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Services.Searching
{
    public class JobSearchResultItem : ISearchResultItem
    {
        private const string type = "Job";

        public virtual string Id { get; set; }
        public string Type { get { return type; } }
        public string Description { get { return string.Format("{0} ({1}; {2})", this.Id, this.UserId, this.DeviceSerialNumber); } }
        public string ScoreValue { get { return string.Format("{0} {1} {2} {3}", this.Id, this.UserId, this.DeviceSerialNumber, this.UserDisplayName); } }

        public string DeviceSerialNumber { get; set; }

        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
    }
}
