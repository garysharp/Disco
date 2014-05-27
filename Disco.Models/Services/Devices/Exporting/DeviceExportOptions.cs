using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Services.Devices.Exporting
{
    public class DeviceExportOptions
    {
        public DeviceExportTypes ExportType { get; set; }
        public int? ExportTypeTargetId { get; set; }
        
        /// <summary>
        /// Adds '=' to the beginning of the string to stop Excel removing the leading zeros
        /// </summary>
        public bool ExcelCsvFormat { get; set; }

        // Device
        [Display(ShortName = "Device", Name = "Serial Number", Description = "The device serial number")]
        public bool DeviceSerialNumber { get; set; }
        [Display(ShortName = "Device", Name = "Asset Number", Description = "The device asset number")]
        public bool DeviceAssetNumber { get; set; }
        [Display(ShortName = "Device", Name = "Location", Description = "The device location")]
        public bool DeviceLocation { get; set; }
        [Display(ShortName = "Device", Name = "Computer Name", Description = "The device computer name")]
        public bool DeviceComputerName { get; set; }
        [Display(ShortName = "Device", Name = "Last Network Logon", Description = "The last recorded time the device access the network")]
        public bool DeviceLastNetworkLogon { get; set; }
        [Display(ShortName = "Device", Name = "Created Date", Description = "The date the device was created in Disco")]
        public bool DeviceCreatedDate { get; set; }
        [Display(ShortName = "Device", Name = "First Enrolled Date", Description = "The date the device was first enrolled in Disco")]
        public bool DeviceFirstEnrolledDate { get; set; }
        [Display(ShortName = "Device", Name = "Last Enrolled Date", Description = "The date the device was last enrolled in Disco")]
        public bool DeviceLastEnrolledDate { get; set; }
        [Display(ShortName = "Device", Name = "Decommissioned Date", Description = "The date the device was decommissioned in Disco")]
        public bool DeviceDecommissionedDate { get; set; }
        [Display(ShortName = "Device", Name = "Decommissioned Reason", Description = "The reason the device was decommissioned")]
        public bool DeviceDecommissionedReason { get; set; }

        // Details
        [Display(ShortName = "Details", Name = "LAN MAC Address", Description = "The LAN MAC Address associated with the device")]
        public bool DetailLanMacAddress { get; set; }
        [Display(ShortName = "Details", Name = "Wireless LAN MAC Address", Description = "The Wireless LAN MAC Address associated with the device")]
        public bool DetailWLanMacAddress { get; set; }
        [Display(ShortName = "Details", Name = "AC Adapter", Description = "The AC Adapter associated with the device")]
        public bool DetailACAdapter { get; set; }
        [Display(ShortName = "Details", Name = "Battery", Description = "The Battery associated with the device")]
        public bool DetailBattery { get; set; }

        // Model
        [Display(ShortName = "Model", Name = "Identifier", Description = "The identifier of the device model associated with the device")]
        public bool ModelId { get; set; }
        [Display(ShortName = "Model", Name = "Description", Description = "The description of the device model associated with the device")]
        public bool ModelDescription { get; set; }
        [Display(ShortName = "Model", Name = "Manufacturer", Description = "The manufacturer of the device model associated with the device")]
        public bool ModelManufacturer { get; set; }
        [Display(ShortName = "Model", Name = "Model", Description = "The model of the device model associated with the device")]
        public bool ModelModel { get; set; }
        [Display(ShortName = "Model", Name = "Type", Description = "The type of device model associated with the device")]
        public bool ModelType { get; set; }

        // Batch
        [Display(ShortName = "Batch", Name = "Identifier", Description = "The identifier of the device batch associated with the device")]
        public bool BatchId { get; set; }
        [Display(ShortName = "Batch", Name = "Name", Description = "The name of the device batch associated with the device")]
        public bool BatchName { get; set; }
        [Display(ShortName = "Batch", Name = "Purchase Date", Description = "The purchase date of the device batch associated with the device")]
        public bool BatchPurchaseDate { get; set; }
        [Display(ShortName = "Batch", Name = "Supplier", Description = "The supplier of the device batch associated with the device")]
        public bool BatchSupplier { get; set; }
        [Display(ShortName = "Batch", Name = "Unit Cost", Description = "The unit cost of the device batch associated with the device")]
        public bool BatchUnitCost { get; set; }
        [Display(ShortName = "Batch", Name = "Warranty Valid Until Date", Description = "The warranty valid until date of the device batch associated with the device")]
        public bool BatchWarrantyValidUntilDate { get; set; }
        [Display(ShortName = "Batch", Name = "Insured Date", Description = "The insured date of the device batch associated with the device")]
        public bool BatchInsuredDate { get; set; }
        [Display(ShortName = "Batch", Name = "Insurance Supplier", Description = "The insurance supplier of the device batch associated with the device")]
        public bool BatchInsuranceSupplier { get; set; }
        [Display(ShortName = "Batch", Name = "Insured Until Date", Description = "The insured until date of the device batch associated with the device")]
        public bool BatchInsuredUntilDate { get; set; }

        // Profile
        [Display(ShortName = "Profile", Name = "Identifier", Description = "The identifier of the device profile associated with the device")]
        public bool ProfileId { get; set; }
        [Display(ShortName = "Profile", Name = "Name", Description = "The name of the device profile associated with the device")]
        public bool ProfileName { get; set; }
        [Display(ShortName = "Profile", Name = "Short Name", Description = "The short name of the device profile associated with the device")]
        public bool ProfileShortName { get; set; }

        // User
        [Display(ShortName = "Assigned User", Name = "Identifier", Description = "The identifier of the user assigned to the device")]
        public bool AssignedUserId { get; set; }
        [Display(ShortName = "Assigned User", Name = "Assigned Date", Description = "The date the device was assigned to the user")]
        public bool AssignedUserDate { get; set; }
        [Display(ShortName = "Assigned User", Name = "Display Name", Description = "The display name of the user assigned to the device")]
        public bool AssignedUserDisplayName { get; set; }
        [Display(ShortName = "Assigned User", Name = "Surname", Description = "The surname of the user assigned to the device")]
        public bool AssignedUserSurname { get; set; }
        [Display(ShortName = "Assigned User", Name = "Given Name", Description = "The given name of the user assigned to the device")]
        public bool AssignedUserGivenName { get; set; }
        [Display(ShortName = "Assigned User", Name = "Phone Number", Description = "The phone number of the user assigned to the device")]
        public bool AssignedUserPhoneNumber { get; set; }
        [Display(ShortName = "Assigned User", Name = "Email Address", Description = "The email address of the user assigned to the device")]
        public bool AssignedUserEmailAddress { get; set; }

        // Jobs
        [Display(ShortName = "Jobs", Name = "Count", Description = "The total number of jobs associated with the device")]
        public bool JobsTotalCount { get; set; }
        [Display(ShortName = "Jobs", Name = "Count Open", Description = "The total number of open jobs associated with the device")]
        public bool JobsOpenCount { get; set; }

        // Attachments
        [Display(ShortName = "Attachments", Name = "Count", Description = "The number of attachments associated with the device")]
        public bool AttachmentsCount { get; set; }

        // Certificates
        [Display(ShortName = "Certificates", Name = "Name", Description = "The name of the most recently assigned active certificate associated with the device")]
        public bool CertificateName { get; set; }
        [Display(ShortName = "Certificates", Name = "Allocated Date", Description = "The allocated date of the most recently assigned active certificate associated with the device")]
        public bool CertificateAllocatedDate { get; set; }
        [Display(ShortName = "Certificates", Name = "Expiration Date", Description = "The expiration date of the most recently assigned active certificate associated with the device")]
        public bool CertificateExpirationDate { get; set; }
        [Display(ShortName = "Certificates", Name = "Provider Id", Description = "The provider identifier of the most recently assigned active certificate associated with the device")]
        public bool CertificateProviderId { get; set; }


        public static DeviceExportOptions DefaultOptions()
        {
            return new DeviceExportOptions()
            {
                ExportType = DeviceExportTypes.All,
                ExcelCsvFormat = true,
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
