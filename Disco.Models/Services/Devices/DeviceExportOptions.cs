using Disco.Models.Exporting;
using Disco.Models.Services.Exporting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Disco.Models.Services.Devices
{
    public class DeviceExportOptions : IExportOptions
    {
        public int Version { get; set; } = 1;
        public ExportFormat Format { get; set; }

        public DeviceExportTypes ExportType { get; set; }
        public int? ExportTypeTargetId { get; set; }

        // Device
        [Display(GroupName = "Device", Name = "Serial Number", Description = "The device serial number")]
        public bool DeviceSerialNumber { get; set; }
        [Display(GroupName = "Device", Name = "Asset Number", Description = "The device asset number")]
        public bool DeviceAssetNumber { get; set; }
        [Display(GroupName = "Device", Name = "Location", Description = "The device location")]
        public bool DeviceLocation { get; set; }
        [Display(GroupName = "Device", Name = "Computer Name", Description = "The device computer name")]
        public bool DeviceComputerName { get; set; }
        [Display(GroupName = "Device", Name = "Last Network Logon", Description = "The last recorded time the device access the network")]
        public bool DeviceLastNetworkLogon { get; set; }
        [Display(GroupName = "Device", Name = "Created Date", Description = "The date the device was created in Disco ICT")]
        public bool DeviceCreatedDate { get; set; }
        [Display(GroupName = "Device", Name = "First Enrolled Date", Description = "The date the device was first enrolled in Disco ICT")]
        public bool DeviceFirstEnrolledDate { get; set; }
        [Display(GroupName = "Device", Name = "Last Enrolled Date", Description = "The date the device was last enrolled in Disco ICT")]
        public bool DeviceLastEnrolledDate { get; set; }
        [Display(GroupName = "Device", Name = "Enrollment Trusted", Description = "The device is trusted to complete an unauthenticated enrollment")]
        public bool DeviceAllowUnauthenticatedEnrol { get; set; }
        [Display(GroupName = "Device", Name = "Decommissioned Date", Description = "The date the device was decommissioned in Disco ICT")]
        public bool DeviceDecommissionedDate { get; set; }
        [Display(GroupName = "Device", Name = "Decommissioned Reason", Description = "The reason the device was decommissioned")]
        public bool DeviceDecommissionedReason { get; set; }

        // Model
        [Display(GroupName = "Model", Name = "Identifier", Description = "The identifier of the device model associated with the device")]
        public bool ModelId { get; set; }
        [Display(GroupName = "Model", Name = "Description", Description = "The description of the device model associated with the device")]
        public bool ModelDescription { get; set; }
        [Display(GroupName = "Model", Name = "Manufacturer", Description = "The manufacturer of the device model associated with the device")]
        public bool ModelManufacturer { get; set; }
        [Display(GroupName = "Model", Name = "Model", Description = "The model of the device model associated with the device")]
        public bool ModelModel { get; set; }
        [Display(GroupName = "Model", Name = "Type", Description = "The type of device model associated with the device")]
        public bool ModelType { get; set; }

        // Batch
        [Display(GroupName = "Batch", Name = "Identifier", Description = "The identifier of the device batch associated with the device")]
        public bool BatchId { get; set; }
        [Display(GroupName = "Batch", Name = "Name", Description = "The name of the device batch associated with the device")]
        public bool BatchName { get; set; }
        [Display(GroupName = "Batch", Name = "Purchase Date", Description = "The purchase date of the device batch associated with the device")]
        public bool BatchPurchaseDate { get; set; }
        [Display(GroupName = "Batch", Name = "Supplier", Description = "The supplier of the device batch associated with the device")]
        public bool BatchSupplier { get; set; }
        [Display(GroupName = "Batch", Name = "Unit Cost", Description = "The unit cost of the device batch associated with the device")]
        public bool BatchUnitCost { get; set; }
        [Display(GroupName = "Batch", Name = "Warranty Valid Until Date", Description = "The warranty valid until date of the device batch associated with the device")]
        public bool BatchWarrantyValidUntilDate { get; set; }
        [Display(GroupName = "Batch", Name = "Insured Date", Description = "The insured date of the device batch associated with the device")]
        public bool BatchInsuredDate { get; set; }
        [Display(GroupName = "Batch", Name = "Insurance Supplier", Description = "The insurance supplier of the device batch associated with the device")]
        public bool BatchInsuranceSupplier { get; set; }
        [Display(GroupName = "Batch", Name = "Insured Until Date", Description = "The insured until date of the device batch associated with the device")]
        public bool BatchInsuredUntilDate { get; set; }

        // Profile
        [Display(GroupName = "Profile", Name = "Identifier", Description = "The identifier of the device profile associated with the device")]
        public bool ProfileId { get; set; }
        [Display(GroupName = "Profile", Name = "Name", Description = "The name of the device profile associated with the device")]
        public bool ProfileName { get; set; }
        [Display(GroupName = "Profile", Name = "Short Name", Description = "The short name of the device profile associated with the device")]
        public bool ProfileShortName { get; set; }

        // User
        [Display(GroupName = "Assigned User", Name = "Identifier", Description = "The identifier of the user assigned to the device")]
        public bool AssignedUserId { get; set; }
        [Display(GroupName = "Assigned User", Name = "Assigned Date", Description = "The date the device was assigned to the user")]
        public bool AssignedUserDate { get; set; }
        [Display(GroupName = "Assigned User", Name = "Display Name", Description = "The display name of the user assigned to the device")]
        public bool AssignedUserDisplayName { get; set; }
        [Display(GroupName = "Assigned User", Name = "Surname", Description = "The surname of the user assigned to the device")]
        public bool AssignedUserSurname { get; set; }
        [Display(GroupName = "Assigned User", Name = "Given Name", Description = "The given name of the user assigned to the device")]
        public bool AssignedUserGivenName { get; set; }
        [Display(GroupName = "Assigned User", Name = "Phone Number", Description = "The phone number of the user assigned to the device")]
        public bool AssignedUserPhoneNumber { get; set; }
        [Display(GroupName = "Assigned User", Name = "Email Address", Description = "The email address of the user assigned to the device")]
        public bool AssignedUserEmailAddress { get; set; }
        public List<string> UserDetailCustom { get; set; } = new List<string>();

        // Jobs
        [Display(GroupName = "Jobs", Name = "Count", Description = "The total number of jobs associated with the device")]
        public bool JobsTotalCount { get; set; }
        [Display(GroupName = "Jobs", Name = "Count Open", Description = "The total number of open jobs associated with the device")]
        public bool JobsOpenCount { get; set; }

        // Attachments
        [Display(GroupName = "Attachments", Name = "Count", Description = "The number of attachments associated with the device")]
        public bool AttachmentsCount { get; set; }

        // Certificates
        [Display(GroupName = "Certificates", Name = "Certificates", Description = "The assigned active certificates associated with the device")]
        public bool Certificates { get; set; }

        // Details
        [Display(GroupName = "Details", Name = "BIOS", Description = "The BIOS associated with the device")]
        public bool DetailBios { get; set; }
        [Display(GroupName = "Details", Name = "Base Board", Description = "The Base Board associated with the device")]
        public bool DetailBaseBoard { get; set; }
        [Display(GroupName = "Details", Name = "System", Description = "The System information associated with the device")]
        public bool DetailComputerSystem { get; set; }
        [Display(GroupName = "Details", Name = "Processors", Description = "The CPU Processors associated with the device")]
        public bool DetailProcessors { get; set; }
        [Display(GroupName = "Details", Name = "Memory", Description = "The Memory/RAM associated with the device")]
        public bool DetailMemory { get; set; }
        [Display(GroupName = "Details", Name = "Disk Drives", Description = "The Disk Drives associated with the device")]
        public bool DetailDiskDrives { get; set; }
        [Display(GroupName = "Details", Name = "LAN Adapters", Description = "The LAN Adapters associated with the device")]
        public bool DetailLanAdapters { get; set; }
        [Display(GroupName = "Details", Name = "Wireless LAN Adapters", Description = "The Wireless LAN Adapters associated with the device")]
        public bool DetailWLanAdapters { get; set; }
        [Display(GroupName = "Details", Name = "MDM Hardware Data", Description = "The Mobile Device Management Hardware Data associated with the device")]
        public bool DetailMdmHardwareData { get; set; }
        [Display(GroupName = "Details", Name = "AC Adapter", Description = "The AC Adapter associated with the device")]
        public bool DetailACAdapter { get; set; }
        [Display(GroupName = "Details", Name = "Battery", Description = "The manually entered battery associated with the device")]
        public bool DetailBatteries { get; set; }
        [Display(GroupName = "Details", Name = "Batteries", Description = "The reported batteries associated with the device")]
        public bool DetailBattery { get; set; }
        [Display(GroupName = "Details", Name = "Keyboard", Description = "The Keyboard associated with the device")]
        public bool DetailKeyboard { get; set; }

        public static DeviceExportOptions DefaultOptions()
        {
            return new DeviceExportOptions()
            {
                ExportType = DeviceExportTypes.All,
                Format = ExportFormat.Xlsx,
                DeviceSerialNumber = true,
                ModelId = true,
                ProfileId = true,
                BatchId = true,
                AssignedUserId = true,
                DeviceLocation = true,
                DeviceAssetNumber = true
            };
        }
    }
}
