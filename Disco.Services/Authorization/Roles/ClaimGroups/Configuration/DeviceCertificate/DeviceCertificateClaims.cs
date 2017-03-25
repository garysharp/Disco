namespace Disco.Services.Authorization.Roles.ClaimGroups.Configuration.DeviceCertificate
{
    [ClaimDetails("Device Certificates", "Permissions related to Device Certificates")]
    public class DeviceCertificateClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Download Certificates", "Can download certificates")]
        public bool DownloadCertificates { get; set; }
    }
}
