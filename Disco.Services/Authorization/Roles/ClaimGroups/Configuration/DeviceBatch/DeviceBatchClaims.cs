namespace Disco.Services.Authorization.Roles.ClaimGroups.Configuration.DeviceBatch
{
    [ClaimDetails("Device Batches", "Permissions related to Device Batches")]
    public class DeviceBatchClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Configure Device Batches", "Can configure device batches")]
        public bool Configure { get; set; }

        [ClaimDetails("Create Device Batches", "Can create device batches")]
        public bool Create { get; set; }

        [ClaimDetails("Delete Device Batches", "Can delete device batches")]
        public bool Delete { get; set; }

        [ClaimDetails("Show Device Batches", "Can show device batches")]
        public bool Show { get; set; }

        [ClaimDetails("Show Timeline", "Can show device batch timeline")]
        public bool ShowTimeline { get; set; }
    }
}
