using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization.Roles.ClaimGroups.Configuration.DeviceCertificate
{
    [ClaimDetails("Device Certificate", "Permissions related to Device Certificates")]
    public class DeviceCertificateClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Download Certificates", "Can download certificates")]
        public bool DownloadCertificates { get; set; }
    }
}
