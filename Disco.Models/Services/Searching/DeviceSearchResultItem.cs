using Disco.Models.Repository;
using System;
using System.Collections.Generic;

namespace Disco.Models.Services.Searching
{
    public class DeviceSearchResultItem : ISearchResultItem
    {
        private const string type = "Device";
        private Lazy<string[]> LazyScoreValue;

        public DeviceSearchResultItem()
        {
            this.LazyScoreValue = new Lazy<string[]>(BuildScoreValues, false);
        }

        public string Id { get; set; }
        public string Type { get { return type; } }
        public string Description { get { return string.Format("{0} ({1})", this.Id, this.ComputerName); } }
        public string[] ScoreValues { get { return LazyScoreValue.Value; } }

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
        public string DeviceProfileName { get; set; }
        public string DeviceBatchName { get; set; }
        public int JobCount { get; set; }
        public DateTime? DecommissionedDate { get; set; }
        public IList<DeviceFlagAssignment> DeviceFlagAssignments { get; set; }

        private string[] BuildScoreValues()
        {
            if (this.AssignedUserId == null)
            {
                return new string[] {
                    this.Id,
                    this.AssetNumber,
                    this.ComputerName
                };
            }
            else
            {
                return new string[] {
                    this.Id,
                    this.AssetNumber,
                    this.ComputerName,
                    this.AssignedUserId.Substring(this.AssignedUserId.IndexOf('\\') + 1),
                    this.AssignedUserId,
                    this.AssignedUserDisplayName
                };
            }
        }
    }
}
