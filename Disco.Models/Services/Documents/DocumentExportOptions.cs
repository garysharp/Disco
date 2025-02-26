using Disco.Models.Exporting;
using Disco.Models.Services.Exporting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Disco.Models.Services.Documents
{
    public class DocumentExportOptions : IExportOptions
    {
        public int Version { get; set; } = 1;
        public ExportFormat Format { get; set; }

        [Required]
        public List<string> DocumentTemplateIds { get; set; } = new List<string>();

        public bool LatestOnly { get; set; }

        // Document Template
        [Display(GroupName = "Document Template", Name = "Identifier", Description = "The identifier of the document template")]
        public bool Id { get; set; }
        [Display(GroupName = "Document Template", Name = "Description", Description = "The description of the document template")]
        public bool Description { get; set; }
        [Display(GroupName = "Document Template", Name = "Scope", Description = "The scope of the document template")]
        public bool Scope { get; set; }

        // Attachment
        [Display(GroupName = "Attachment", Name = "Identifier", Description = "The identifier of the document instance")]
        public bool AttachmentId { get; set; }
        [Display(GroupName = "Attachment", Name = "Created Date", Description = "The date the document instance was created")]
        public bool AttachmentCreatedDate { get; set; }
        [Display(GroupName = "Attachment", Name = "Created User", Description = "The user who created the document instance")]
        public bool AttachmentCreatedUser { get; set; }
        [Display(GroupName = "Attachment", Name = "Filename", Description = "The filename of the document instance")]
        public bool AttachmentFilename { get; set; }
        [Display(GroupName = "Attachment", Name = "Mime Type", Description = "The mime type of the document instance")]
        public bool AttachmentMimeType { get; set; }
        [Display(GroupName = "Attachment", Name = "Comments", Description = "The comments of the document instance")]
        public bool AttachmentComments { get; set; }

        // Device
        [Display(GroupName = "Device", Name = "Serial Number", Description = "The serial number of the device associated with the document instance")]
        public bool DeviceSerialNumber { get; set; }
        [Display(GroupName = "Device", Name = "Asset Number", Description = "The asset number of the device associated with the document instance")]
        public bool DeviceAssetNumber { get; set; }
        [Display(GroupName = "Device", Name = "Location", Description = "The location of the device associated with the document instance")]
        public bool DeviceLocation { get; set; }
        [Display(GroupName = "Device", Name = "Computer Name", Description = "The computer name of the device associated with the document instance")]
        public bool DeviceComputerName { get; set; }
        [Display(GroupName = "Device", Name = "Last Network Logon", Description = "The last recorded time that the device associated with the document instance accessed the network")]
        public bool DeviceLastNetworkLogon { get; set; }
        [Display(GroupName = "Device", Name = "Created Date", Description = "The date that the device associated with the document instance was created in Disco ICT")]
        public bool DeviceCreatedDate { get; set; }
        [Display(GroupName = "Device", Name = "First Enrolled Date", Description = "The date that the device associated with the document instance was first enrolled in Disco ICT")]
        public bool DeviceFirstEnrolledDate { get; set; }
        [Display(GroupName = "Device", Name = "Last Enrolled Date", Description = "The date that the device associated with the document instance was last enrolled in Disco ICT")]
        public bool DeviceLastEnrolledDate { get; set; }
        [Display(GroupName = "Device", Name = "Enrolment Trusted", Description = "Whether the device associated with the document instance is trusted to complete an unauthenticated enrolment")]
        public bool DeviceAllowUnauthenticatedEnrol { get; set; }
        [Display(GroupName = "Device", Name = "Decommissioned Date", Description = "The date that the device associated with the document instance was decommissioned in Disco ICT")]
        public bool DeviceDecommissionedDate { get; set; }
        [Display(GroupName = "Device", Name = "Decommissioned Reason", Description = "The reason that the device associated with the document instance was decommissioned")]
        public bool DeviceDecommissionedReason { get; set; }

        public bool HasDeviceOptions()
            => DeviceSerialNumber || DeviceAssetNumber || DeviceLocation || DeviceComputerName ||
            DeviceLastNetworkLogon || DeviceCreatedDate || DeviceFirstEnrolledDate || DeviceLastEnrolledDate ||
            DeviceAllowUnauthenticatedEnrol || DeviceDecommissionedDate || DeviceDecommissionedReason;

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
            => ModelId || ModelDescription || ModelManufacturer || ModelModel || ModelType;

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
            => BatchId || BatchName || BatchPurchaseDate || BatchSupplier || BatchUnitCost ||
            BatchWarrantyValidUntilDate || BatchInsuredDate || BatchInsuranceSupplier || BatchInsuredUntilDate;

        // Profile
        [Display(GroupName = "Profile", Name = "Identifier", Description = "The identifier of the device profile associated with the device")]
        public bool ProfileId { get; set; }
        [Display(GroupName = "Profile", Name = "Name", Description = "The name of the device profile associated with the device")]
        public bool ProfileName { get; set; }
        [Display(GroupName = "Profile", Name = "Short Name", Description = "The short name of the device profile associated with the device")]
        public bool ProfileShortName { get; set; }
        public bool HasDeviceProfileOptions()
            => ProfileId || ProfileName || ProfileShortName;

        // Job
        [Display(GroupName = "Job", Name = "Identifier", Description = "The identifier of the job associated with the document instance")]
        public bool JobId { get; set; }
        [Display(GroupName = "Job", Name = "Status", Description = "The status of the job associated with the document instance")]
        public bool JobStatus { get; set; }
        [Display(GroupName = "Job", Name = "Type", Description = "The type of the job associated with the document instance")]
        public bool JobType { get; set; }
        [Display(GroupName = "Job", Name = "Sub Types", Description = "The sub types of the job associated with the document instance")]
        public bool JobSubTypes { get; set; }
        [Display(GroupName = "Job", Name = "Opened Date", Description = "The date the job was opened associated with the document instance")]
        public bool JobOpenedDate { get; set; }
        [Display(GroupName = "Job", Name = "Opened User", Description = "The user who opened the job associated with the document instance")]
        public bool JobOpenedUser { get; set; }
        [Display(GroupName = "Job", Name = "Expected Closed Date", Description = "The expected closed date of the job associated with the document instance")]
        public bool JobExpectedClosedDate { get; set; }
        [Display(GroupName = "Job", Name = "Closed Date", Description = "The date the job was closed associated with the document instance")]
        public bool JobClosedDate { get; set; }
        [Display(GroupName = "Job", Name = "Closed User", Description = "The user who closed the job associated with the document instance")]
        public bool JobClosedUser { get; set; }
        public bool HasJobOptions()
            => JobId || JobStatus || JobType || JobSubTypes || JobOpenedDate || JobOpenedUser ||
                JobExpectedClosedDate || JobClosedDate || JobClosedUser;

        // User
        [Display(GroupName = "User", Name = "Identifier", Description = "The identifier of the user associated with the document instance")]
        public bool UserId { get; set; }
        [Display(GroupName = "User", Name = "Display Name", Description = "The display name of the user associated with the document instance")]
        public bool UserDisplayName { get; set; }
        [Display(GroupName = "User", Name = "Surname", Description = "The surname of the user associated with the document instance")]
        public bool UserSurname { get; set; }
        [Display(GroupName = "User", Name = "Given Name", Description = "The given name of the user associated with the document instance")]
        public bool UserGivenName { get; set; }
        [Display(GroupName = "User", Name = "Phone Number", Description = "The phone number of the user associated with the document instance")]
        public bool UserPhoneNumber { get; set; }
        [Display(GroupName = "User", Name = "Email Address", Description = "The email address of the user associated with the document instance")]
        public bool UserEmailAddress { get; set; }
        public List<string> UserDetailCustom { get; set; } = new List<string>();
        public bool HasUserOptions()
            => UserDisplayName || UserSurname || UserGivenName || UserPhoneNumber || UserEmailAddress || (UserDetailCustom?.Any() ?? false);

        public static DocumentExportOptions DefaultOptions()
        {
            return new DocumentExportOptions()
            {
                Format = ExportFormat.Xlsx,
                LatestOnly = true,
                Id = true,
                Description = true,
                Scope = true,
                AttachmentId = true,
                AttachmentCreatedUser = true,
                AttachmentCreatedDate = true,
                AttachmentComments = true,
                DeviceSerialNumber = true,
                JobId = true,
                JobStatus = true,
                JobType = true,
                UserId = true,
                UserDisplayName = true,
            };
        }
    }
}
