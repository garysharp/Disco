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
            LazyScoreValue = new Lazy<string[]>(BuildScoreValues, false);
        }

        public string Id { get; set; }
        public string Type { get { return type; } }
        public string Description { get { return $"{Id} ({ComputerName})"; } }
        public string[] ScoreValues { get { return LazyScoreValue.Value; } }

        public string AssetNumber { get; set; }
        public string AssignedUserDescription
        {
            get
            {
                if (AssignedUserId != null)
                {
                    if (AssignedUserDisplayName != null)
                        return $"{AssignedUserDisplayName} ({AssignedUserId})";
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
            if (AssignedUserId == null)
            {
                return new string[] {
                    Id,
                    AssetNumber,
                    ComputerName
                };
            }
            else
            {
                return new string[] {
                    Id,
                    AssetNumber,
                    ComputerName,
                    AssignedUserId.Substring(AssignedUserId.IndexOf('\\') + 1),
                    AssignedUserId,
                    AssignedUserDisplayName
                };
            }
        }
    }
}
