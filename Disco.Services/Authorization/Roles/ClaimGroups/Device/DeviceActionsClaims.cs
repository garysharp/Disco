using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization.Roles.ClaimGroups.Device
{
    [ClaimDetails("Actions", "Permissions related to Device Actions")]
    public class DeviceActionsClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Allow Unauthenticated Enrol", "Can allow devices to enrol without authentication")]
        public bool AllowUnauthenticatedEnrol { get; set; }

        [ClaimDetails("Assign User", "Can update the user assignment of devices")]
        public bool AssignUser { get; set; }

        [ClaimDetails("Decommission", "Can decommission devices")]
        public bool Decommission { get; set; }
        [ClaimDetails("Recommission", "Can recommission devices")]
        public bool Recommission { get; set; }
        [ClaimDetails("Delete", "Can delete devices")]
        public bool Delete { get; set; }

        [ClaimDetails("Add Attachments", "Can add attachments to devices")]
        public bool AddAttachments { get; set; }
        [ClaimDetails("Remove Any Attachments", "Can remove any attachments from devices")]
        public bool RemoveAnyAttachments { get; set; }
        [ClaimDetails("Remove Own Attachments", "Can remove own attachments from devices")]
        public bool RemoveOwnAttachments { get; set; }

        [ClaimDetails("Generate Documents", "Can generate documents for jobs")]
        public bool GenerateDocuments { get; set; }

        [ClaimDetails("Enrol Devices", "Can add devices offline and enrol devices with the Bootstrapper")]
        public bool EnrolDevices { get; set; }
        [ClaimDetails("Import Devices", "Can bulk import devices")]
        public bool Import { get; set; }
        [ClaimDetails("Export Devices", "Can export devices in a bulk format")]
        public bool Export { get; set; }
    }
}
