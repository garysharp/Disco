using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Services.Searching
{
    public class DeviceSearchResultItem : ISearchResultItem
    {
        private const string type = "Device";

        public string Id { get; set; }
        public string Type { get { return type; } }
        public string Description { get { return string.Format("{0} ({1})", this.Id, this.ComputerName); } }
        public string ScoreValue { get { return string.Format("{0} {1} {2} {3} {4}", this.Id, this.AssignedUserId.Substring(0, this.AssignedUserId.IndexOf('\\')), this.AssignedUserId, this.AssignedUserDisplayName, this.AssetNumber); } }

        public string AssetNumber { get; set; }
        public string AssignedUserDescription
        {
            get
            {
                if (AssignedUserId != null)
                {
                    if (AssignedUserDisplayName != null)
                        return string.Format("{0} ({1})", AssignedUserDisplayName, AssignedUserId);
                    else
                        return AssignedUserId;
                }
                return string.Empty;
            }
        }
        public string AssignedUserDisplayName { get; set; }
        public string AssignedUserId { get; set; }
        public string ComputerName { get; set; }
        public string DeviceModelDescription { get; set; }
        public string DeviceProfileDescription { get; set; }
        public int JobCount { get; set; }
        public DateTime? DecommissionedDate { get; set; }
    }
}
