using Disco.Models.Exporting;
using Disco.Models.Services.Exporting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Disco.Models.Services.Devices.DeviceFlag
{
    public class DeviceFlagExportOptions : IExportOptions
    {
        public int Version { get; set; } = 1;
        public ExportFormat Format { get; set; }

        [Required]
        public List<int> DeviceFlagIds { get; set; } = new List<int>();

        [Display(Name = "Current Only")]
        public bool CurrentOnly { get; set; }

        // Device Flag
        [Display(GroupName = "Device Flag", Name = "Identifier", Description = "The identifier of the device flag")]
        public bool Id { get; set; }
        [Display(GroupName = "Device Flag", Name = "Name", Description = "The name of the device flag")]
        public bool Name { get; set; }
        [Display(GroupName = "Device Flag", Name = "Description", Description = "The description of the device flag")]
        public bool Description { get; set; }
        [Display(GroupName = "Device Flag", Name = "Icon", Description = "The icon assigned to the device flag")]
        public bool Icon { get; set; }
        [Display(GroupName = "Device Flag", Name = "Icon Colour", Description = "The icon colour assigned to the device flag")]
        public bool IconColour { get; set; }
        [Display(GroupName = "Device Flag", Name = "Assignment Identifier", Description = "The identifier of the device flag assignment")]
        public bool AssignmentId { get; set; }
        [Display(GroupName = "Device Flag", Name = "Added Date", Description = "The date the device flag was assigned to the user")]
        public bool AddedDate { get; set; }
        [Display(GroupName = "Device Flag", Name = "Added User Identifier", Description = "The identifier of the user who assigned the device flag")]
        public bool AddedUserId { get; set; }
        [Display(GroupName = "Device Flag", Name = "Removed Date", Description = "The date the device flag was unassigned from the user")]
        public bool RemovedDate { get; set; }
        [Display(GroupName = "Device Flag", Name = "Removed User Identifier", Description = "The identifier of the user who unassigned the device flag")]
        public bool RemovedUserId { get; set; }
        [Display(GroupName = "Device Flag", Name = "Comments", Description = "The comments associated with the device flag assignment")]
        public bool Comments { get; set; }

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
        [Display(GroupName = "Device", Name = "Enrolment Trusted", Description = "The device is trusted to complete an unauthenticated enrolment")]
        public bool DeviceAllowUnauthenticatedEnrol { get; set; }
        [Display(GroupName = "Device", Name = "Decommissioned Date", Description = "The date the device was decommissioned in Disco ICT")]
        public bool DeviceDecommissionedDate { get; set; }
        [Display(GroupName = "Device", Name = "Decommissioned Reason", Description = "The reason the device was decommissioned")]
        public bool DeviceDecommissionedReason { get; set; }

        public bool HasDeviceOptions()
        {
            return DeviceSerialNumber ||
                DeviceAssetNumber ||
                DeviceLocation ||
                DeviceComputerName ||
                DeviceLastNetworkLogon ||
                DeviceCreatedDate ||
                DeviceFirstEnrolledDate ||
                DeviceLastEnrolledDate ||
                DeviceAllowUnauthenticatedEnrol ||
                DeviceDecommissionedDate ||
                DeviceDecommissionedReason;
        }

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
        public bool HasDeviceModelOptions()
        {
            return ModelId ||
                ModelDescription ||
                ModelManufacturer ||
                ModelModel ||
                ModelType;
        }

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
        public bool HasDeviceBatchOptions()
        {
            return BatchId ||
                BatchName ||
                BatchPurchaseDate ||
                BatchSupplier ||
                BatchUnitCost ||
                BatchWarrantyValidUntilDate ||
                BatchInsuredDate ||
                BatchInsuranceSupplier ||
                BatchInsuredUntilDate;
        }

        // Profile
        [Display(GroupName = "Profile", Name = "Identifier", Description = "The identifier of the device profile associated with the device")]
        public bool ProfileId { get; set; }
        [Display(GroupName = "Profile", Name = "Name", Description = "The name of the device profile associated with the device")]
        public bool ProfileName { get; set; }
        [Display(GroupName = "Profile", Name = "Short Name", Description = "The short name of the device profile associated with the device")]
        public bool ProfileShortName { get; set; }
        public bool HasDeviceProfileOptions()
        {
            return ProfileId ||
                ProfileName ||
                ProfileShortName;
        }

        // Assigned User
        [Display(GroupName = "Assigned User", Name = "Identifier", Description = "The identifier of the user assigned to the device flag")]
        public bool AssignedUserId { get; set; }
        [Display(GroupName = "Assigned User", Name = "Display Name", Description = "The display name of the user assigned to the device flag")]
        public bool AssignedUserDisplayName { get; set; }
        [Display(GroupName = "Assigned User", Name = "Surname", Description = "The surname of the user assigned to the device flag")]
        public bool AssignedUserSurname { get; set; }
        [Display(GroupName = "Assigned User", Name = "Given Name", Description = "The given name of the user assigned to the device flag")]
        public bool AssignedUserGivenName { get; set; }
        [Display(GroupName = "Assigned User", Name = "Phone Number", Description = "The phone number of the user assigned to the device flag")]
        public bool AssignedUserPhoneNumber { get; set; }
        [Display(GroupName = "Assigned User", Name = "Email Address", Description = "The email address of the user assigned to the device flag")]
        public bool AssignedUserEmailAddress { get; set; }
        public List<string> UserDetailCustom { get; set; } = new List<string>();
        public bool HasAssignedUserOptions()
        {
            return AssignedUserId ||
                AssignedUserDisplayName ||
                AssignedUserSurname ||
                AssignedUserGivenName ||
                AssignedUserPhoneNumber ||
                AssignedUserEmailAddress;
        }

        public static DeviceFlagExportOptions DefaultOptions()
        {
            return new DeviceFlagExportOptions()
            {
                Format = ExportFormat.Xlsx,
                CurrentOnly = true,
                Name = true,
                AddedDate = true,
                DeviceSerialNumber = true,
                ModelDescription = true,
                ProfileName = true,
                AssignedUserId = true,
                AssignedUserDisplayName = true,
                Comments = true,
            };
        }
    }
}
