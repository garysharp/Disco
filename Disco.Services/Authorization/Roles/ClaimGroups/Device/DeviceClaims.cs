﻿namespace Disco.Services.Authorization.Roles.ClaimGroups.Device
{
    [ClaimDetails("Device", "Permissions related to Devices")]
    public class DeviceClaims : BaseRoleClaimGroup
    {
        public DeviceClaims()
        {
            Properties = new DevicePropertiesClaims();
            Actions = new DeviceActionsClaims();
        }

        [ClaimDetails("Search Devices", "Can search devices")]
        public bool Search { get; set; }

        [ClaimDetails("Show Devices", "Can show devices")]
        public bool Show { get; set; }

        [ClaimDetails("Show Details", "Can show details associated with devices")]
        public bool ShowDetails { get; set; }
        [ClaimDetails("Show Attachments", "Can show device attachments")]
        public bool ShowAttachments { get; set; }
        [ClaimDetails("Show Certificates", "Can show certificates associated with devices")]
        public bool ShowCertificates { get; set; }
        [ClaimDetails("Show Devices Jobs", "Can show jobs associated with devices")]
        public bool ShowJobs { get; set; }
        [ClaimDetails("Show Assignment History", "Can show the assignment history for devices")]
        public bool ShowAssignmentHistory { get; set; }
        [ClaimDetails("Show Device Flag Assignments", "Can show flags associated with devices")]
        public bool ShowFlagAssignments { get; set; }

        public DevicePropertiesClaims Properties { get; set; }

        public DeviceActionsClaims Actions { get; set; }
    }
}
