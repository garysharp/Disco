using System;

namespace Disco.Models.BI.Search
{
    public class DeviceSearchResultItem
    {
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
        public string SerialNumber { get; set; }
    }
}
