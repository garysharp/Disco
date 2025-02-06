using Disco.Models.Exporting;
using Disco.Models.Services.Exporting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Disco.Models.Services.Jobs
{
    public class JobExportOptions : IExportOptions
    {
        public int Version { get; set; } = 1;
        public ExportFormat Format { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, HtmlEncode = false)]
        public DateTime FilterStartDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, HtmlEncode = false)]
        public DateTime? FilterEndDate { get; set; }
        public string FilterJobStatusId { get; set; }
        public string FilterJobTypeId { get; set; }
        public List<string> FilterJobSubTypeIds { get; set; }
        public int? FilterJobQueueId { get; set; }

        // Job
        [Display(ShortName = "Job", Name = "Identifier", Description = "The identifier of the job")]
        public bool JobId { get; set; }
        [Display(ShortName = "Job", Name = "Status", Description = "The status of the job")]
        public bool JobStatus { get; set; }
        [Display(ShortName = "Job", Name = "Type", Description = "The type of the job")]
        public bool JobType { get; set; }
        [Display(ShortName = "Job", Name = "Sub Types", Description = "The sub types of the job")]
        public bool JobSubTypes { get; set; }
        [Display(ShortName = "Job", Name = "Opened Date", Description = "The date the job was opened")]
        public bool JobOpenedDate { get; set; }
        [Display(ShortName = "Job", Name = "Opened User", Description = "The user who opened the job")]
        public bool JobOpenedUser { get; set; }
        [Display(ShortName = "Job", Name = "Expected Closed Date", Description = "The expected closed date of the job")]
        public bool JobExpectedClosedDate { get; set; }
        [Display(ShortName = "Job", Name = "Closed Date", Description = "The date the job was closed")]
        public bool JobClosedDate { get; set; }
        [Display(ShortName = "Job", Name = "Closed User", Description = "The user who closed the job")]
        public bool JobClosedUser { get; set; }

        // Job Details
        [Display(ShortName = "Job Details", Name = "Held Date", Description = "The date the device was held")]
        public bool JobDeviceHeldDate { get; set; }
        [Display(ShortName = "Job Details", Name = "Held User", Description = "The user who held the device")]
        public bool JobDeviceHeldUser { get; set; }
        [Display(ShortName = "Job Details", Name = "Held Location", Description = "The location the device was held")]
        public bool JobDeviceHeldLocation { get; set; }
        [Display(ShortName = "Job Details", Name = "Ready For Return Date", Description = "The date the device was ready for return")]
        public bool JobDeviceReadyForReturnDate { get; set; }
        [Display(ShortName = "Job Details", Name = "Ready For Return User", Description = "The user who made the device ready for return")]
        public bool JobDeviceReadyForReturnUser { get; set; }
        [Display(ShortName = "Job Details", Name = "Returned Date", Description = "The date the device was returned")]
        public bool JobDeviceReturnedDate { get; set; }
        [Display(ShortName = "Job Details", Name = "Returned User", Description = "The user who returned the device")]
        public bool JobDeviceReturnedUser { get; set; }
        [Display(ShortName = "Job Details", Name = "Waiting For User Action Date", Description = "The date the job was waiting for user action")]
        public bool JobWaitingForUserActionDate { get; set; }

        // Job Log
        [Display(ShortName = "Job Log", Name = "Count", Description = "The number of log entries for the job")]
        public bool LogCount { get; set; }
        [Display(ShortName = "Job Log", Name = "First Date", Description = "The date of the first log entry for the job")]
        public bool LogFirstDate { get; set; }
        [Display(ShortName = "Job Log", Name = "First User", Description = "The user who made the first log entry for the job")]
        public bool LogFirstUser { get; set; }
        [Display(ShortName = "Job Log", Name = "First Content", Description = "The content of the first log entry for the job")]
        public bool LogFirstContent { get; set; }
        [Display(ShortName = "Job Log", Name = "Last Date", Description = "The date of the last log entry for the job")]
        public bool LogLastDate { get; set; }
        [Display(ShortName = "Job Log", Name = "Last User", Description = "The user who made the last log entry for the job")]
        public bool LogLastUser { get; set; }
        [Display(ShortName = "Job Log", Name = "Last Content", Description = "The content of the last log entry for the job")]
        public bool LogLastContent { get; set; }

        // Job Attachments
        [Display(ShortName = "Job Attachments", Name = "Count", Description = "The number of attachments for the job")]
        public bool AttachmentsCount { get; set; }

        // Job Queues
        [Display(ShortName = "Job Queues", Name = "Count", Description = "The number of times the job has been associated with a queue")]
        public bool JobQueueCount { get; set; }
        [Display(ShortName = "Job Queues", Name = "Active Count", Description = "The number of active queues the job is associated with")]
        public bool JobQueueActiveCount { get; set; }
        [Display(ShortName = "Job Queues", Name = "Active Latest", Description = "The latest queue the job is associated with")]
        public bool JobQueueActiveLatest { get; set; }
        [Display(ShortName = "Job Queues", Name = "Active Latest Date", Description = "The date the latest queue was added")]
        public bool JobQueueActiveLatestAddedDate { get; set; }
        [Display(ShortName = "Job Queues", Name = "Active Latest User", Description = "The user who added the latest queue")]
        public bool JobQueueActiveLatestAddedUser { get; set; }

        // Job Type - Warranty
        [Display(ShortName = "Job Warranty", Name = "External Name", Description = "The name of the external warranty provider")]
        public bool JobWarrantyExternalName { get; set; }
        [Display(ShortName = "Job Warranty", Name = "External Logged Date", Description = "The date the warranty was logged with the external provider")]
        public bool JobWarrantyExternalLoggedDate { get; set; }
        [Display(ShortName = "Job Warranty", Name = "External Reference", Description = "The reference of the warranty with the external provider")]
        public bool JobWarrantyExternalReference { get; set; }
        [Display(ShortName = "Job Warranty", Name = "External Completed Date", Description = "The date the warranty was completed with the external provider")]
        public bool JobWarrantyExternalCompletedDate { get; set; }

        // Job Type - NonWarranty
        [Display(ShortName = "Job Non Warranty", Name = "Accounting Charge Required Date", Description = "The date the accounting charge was required")]
        public bool JobNonWarrantyAccountingChargeRequiredDate { get; set; }
        [Display(ShortName = "Job Non Warranty", Name = "Accounting Charge Added Date", Description = "The date the accounting charge was added")]
        public bool JobNonWarrantyAccountingChargeAddedDate { get; set; }
        [Display(ShortName = "Job Non Warranty", Name = "Accounting Charge Paid Date", Description = "The date the accounting charge was paid")]
        public bool JobNonWarrantyAccountingChargePaidDate { get; set; }
        [Display(ShortName = "Job Non Warranty", Name = "Purchase Order Raised Date", Description = "The date the purchase order was raised")]
        public bool JobNonWarrantyPurchaseOrderRaisedDate { get; set; }
        [Display(ShortName = "Job Non Warranty", Name = "Purchase Order Reference", Description = "The reference of the purchase order")]
        public bool JobNonWarrantyPurchaseOrderReference { get; set; }
        [Display(ShortName = "Job Non Warranty", Name = "Purchase Order Sent Date", Description = "The date the purchase order was sent")]
        public bool JobNonWarrantyPurchaseOrderSentDate { get; set; }
        [Display(ShortName = "Job Non Warranty", Name = "Invoice Received Date", Description = "The date the invoice was received")]
        public bool JobNonWarrantyInvoiceReceivedDate { get; set; }
        [Display(ShortName = "Job Non Warranty", Name = "Repairer Name", Description = "The name of the repairer")]
        public bool JobNonWarrantyRepairerName { get; set; }
        [Display(ShortName = "Job Non Warranty", Name = "Repairer Logged Date", Description = "The date the job was logged with the repairer")]
        public bool JobNonWarrantyRepairerLoggedDate { get; set; }
        [Display(ShortName = "Job Non Warranty", Name = "Repairer Reference", Description = "The repairer reference for the job")]
        public bool JobNonWarrantyRepairerReference { get; set; }
        [Display(ShortName = "Job Non Warranty", Name = "Repairer Completed Date", Description = "The date the repairer completed the job")]
        public bool JobNonWarrantyRepairerCompletedDate { get; set; }

        // Job Type - Insurance
        [Display(ShortName = "Job Insurance", Name = "Loss Or Damage Date", Description = "The date of the loss or damage")]
        public bool JobMetaInsuranceLossOrDamageDate { get; set; }
        [Display(ShortName = "Job Insurance", Name = "Event Location", Description = "The location of the event")]
        public bool JobMetaInsuranceEventLocation { get; set; }
        [Display(ShortName = "Job Insurance", Name = "Description", Description = "The description of the event")]
        public bool JobMetaInsuranceDescription { get; set; }
        [Display(ShortName = "Job Insurance", Name = "Third Party Caused Name", Description = "The name of the third party which caused the event")]
        public bool JobMetaInsuranceThirdPartyCausedName { get; set; }
        [Display(ShortName = "Job Insurance", Name = "Third Party Caused Why", Description = "The reason the third party caused the event")]
        public bool JobMetaInsuranceThirdPartyCausedWhy { get; set; }
        [Display(ShortName = "Job Insurance", Name = "Witnesses Names Addresses", Description = "The names and addresses of the witnesses")]
        public bool JobMetaInsuranceWitnessesNamesAddresses { get; set; }
        [Display(ShortName = "Job Insurance", Name = "Burglary Theft Method Of Entry", Description = "The method of entry for a burglary or theft")]
        public bool JobMetaInsuranceBurglaryTheftMethodOfEntry { get; set; }
        [Display(ShortName = "Job Insurance", Name = "Property Last Seen Date", Description = "The date the property was last seen")]
        public bool JobMetaInsurancePropertyLastSeenDate { get; set; }
        [Display(ShortName = "Job Insurance", Name = "Police Station Notified", Description = "The police station which was notified")]
        public bool JobMetaInsurancePoliceNotifiedStation { get; set; }
        [Display(ShortName = "Job Insurance", Name = "Police Notified Date", Description = "The date the police were notified")]
        public bool JobMetaInsurancePoliceNotifiedDate { get; set; }
        [Display(ShortName = "Job Insurance", Name = "Police Crime Report Number", Description = "The crime report number provided by the police")]
        public bool JobMetaInsurancePoliceNotifiedCrimeReportNo { get; set; }
        [Display(ShortName = "Job Insurance", Name = "Recover Reduce Action", Description = "The action taken to recover or reduce the loss")]
        public bool JobMetaInsuranceRecoverReduceAction { get; set; }
        [Display(ShortName = "Job Insurance", Name = "Other Interested Parties", Description = "Other parties interested in the event")]
        public bool JobMetaInsuranceOtherInterestedParties { get; set; }
        [Display(ShortName = "Job Insurance", Name = "Date Of Purchase", Description = "The date the item was purchased")]
        public bool JobMetaInsuranceDateOfPurchase { get; set; }
        [Display(ShortName = "Job Insurance", Name = "Claim Form Sent Date", Description = "The date the claim form was sent")]
        public bool JobMetaInsuranceClaimFormSentDate { get; set; }
        [Display(ShortName = "Job Insurance", Name = "Insurer", Description = "The insurer associated with the claim")]
        public bool JobMetaInsuranceInsurer { get; set; }
        [Display(ShortName = "Job Insurance", Name = "Insurer Reference", Description = "The reference provided by the insurer")]
        public bool JobMetaInsuranceInsurerReference { get; set; }

        // Job Type = User Management
        [Display(ShortName = "Job User Management", Name = "Flags", Description = "The user management flags associated with the job")]
        public bool JobUserManagementFlags { get; set; }

        // User
        [Display(ShortName = "User", Name = "Identifier", Description = "The identifier of the user associated with the job")]
        public bool UserId { get; set; }
        [Display(ShortName = "User", Name = "Display Name", Description = "The display name of the user associated with the job")]
        public bool UserDisplayName { get; set; }
        [Display(ShortName = "User", Name = "Surname", Description = "The surname of the user associated with the job")]
        public bool UserSurname { get; set; }
        [Display(ShortName = "User", Name = "Given Name", Description = "The given name of the user associated with the job")]
        public bool UserGivenName { get; set; }
        [Display(ShortName = "User", Name = "Phone Number", Description = "The phone number of the user associated with the job")]
        public bool UserPhoneNumber { get; set; }
        [Display(ShortName = "User", Name = "Email Address", Description = "The email address of the user associated with the job")]
        public bool UserEmailAddress { get; set; }
        [Display(ShortName = "User", Name = "Custom Details", Description = "The custom details provided by plugins for the user associated with the job")]
        public bool UserDetailCustom { get; set; }

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
        [Display(ShortName = "Device", Name = "Created Date", Description = "The date the device was created in Disco ICT")]
        public bool DeviceCreatedDate { get; set; }
        [Display(ShortName = "Device", Name = "First Enrolled Date", Description = "The date the device was first enrolled in Disco ICT")]
        public bool DeviceFirstEnrolledDate { get; set; }
        [Display(ShortName = "Device", Name = "Last Enrolled Date", Description = "The date the device was last enrolled in Disco ICT")]
        public bool DeviceLastEnrolledDate { get; set; }
        [Display(ShortName = "Device", Name = "Enrolment Trusted", Description = "The device is trusted to complete an unauthenticated enrolment")]
        public bool DeviceAllowUnauthenticatedEnrol { get; set; }
        [Display(ShortName = "Device", Name = "Decommissioned Date", Description = "The date the device was decommissioned in Disco ICT")]
        public bool DeviceDecommissionedDate { get; set; }
        [Display(ShortName = "Device", Name = "Decommissioned Reason", Description = "The reason the device was decommissioned")]
        public bool DeviceDecommissionedReason { get; set; }

        // Model
        [Display(ShortName = "Device Model", Name = "Identifier", Description = "The identifier of the device model associated with the job")]
        public bool DeviceModelId { get; set; }
        [Display(ShortName = "Device Model", Name = "Description", Description = "The description of the device model associated with the job")]
        public bool DeviceModelDescription { get; set; }
        [Display(ShortName = "Device Model", Name = "Manufacturer", Description = "The manufacturer of the device model associated with the job")]
        public bool DeviceModelManufacturer { get; set; }
        [Display(ShortName = "Device Model", Name = "Model", Description = "The model of the device model associated with the job")]
        public bool DeviceModelModel { get; set; }
        [Display(ShortName = "Device Model", Name = "Type", Description = "The type of device model associated with the job")]
        public bool DeviceModelType { get; set; }

        // Batch
        [Display(ShortName = "Device Batch", Name = "Identifier", Description = "The identifier of the device batch associated with the job")]
        public bool DeviceBatchId { get; set; }
        [Display(ShortName = "Device Batch", Name = "Name", Description = "The name of the device batch associated with the job")]
        public bool DeviceBatchName { get; set; }
        [Display(ShortName = "Device Batch", Name = "Purchase Date", Description = "The purchase date of the device batch associated with the job")]
        public bool DeviceBatchPurchaseDate { get; set; }
        [Display(ShortName = "Device Batch", Name = "Supplier", Description = "The supplier of the device batch associated with the job")]
        public bool DeviceBatchSupplier { get; set; }
        [Display(ShortName = "Device Batch", Name = "Unit Cost", Description = "The unit cost of the device batch associated with the job")]
        public bool DeviceBatchUnitCost { get; set; }
        [Display(ShortName = "Device Batch", Name = "Warranty Valid Until Date", Description = "The warranty valid until date of the device batch associated with the job")]
        public bool DeviceBatchWarrantyValidUntilDate { get; set; }
        [Display(ShortName = "Device Batch", Name = "Insured Date", Description = "The insured date of the device batch associated with the job")]
        public bool DeviceBatchInsuredDate { get; set; }
        [Display(ShortName = "Device Batch", Name = "Insurance Supplier", Description = "The insurance supplier of the device batch associated with the job")]
        public bool DeviceBatchInsuranceSupplier { get; set; }
        [Display(ShortName = "Device Batch", Name = "Insured Until Date", Description = "The insured until date of the device batch associated with the job")]
        public bool DeviceBatchInsuredUntilDate { get; set; }

        // Profile
        [Display(ShortName = "Device Profile", Name = "Identifier", Description = "The identifier of the device profile associated with the job")]
        public bool DeviceProfileId { get; set; }
        [Display(ShortName = "Device Profile", Name = "Name", Description = "The name of the device profile associated with the job")]
        public bool DeviceProfileName { get; set; }
        [Display(ShortName = "Device Profile", Name = "Short Name", Description = "The short name of the device profile associated with the job")]
        public bool DeviceProfileShortName { get; set; }

        public static JobExportOptions DefaultOptions()
        {
            return new JobExportOptions()
            {
                FilterJobStatusId = "Open",
                FilterStartDate = new DateTime(DateTime.Now.Year, 1, 1),
                Format = ExportFormat.Xlsx,
                JobId = true,
                JobStatus = true,
                JobType = true,
                JobSubTypes = true,
                JobOpenedDate = true,
                DeviceSerialNumber = true,
                DeviceModelDescription = true,
                DeviceProfileName = true,
                UserId = true,
                UserDisplayName = true,
            };
        }
    }
}
