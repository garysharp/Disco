using Disco.Models.Exporting;
using Disco.Models.Repository;
using System;
using System.Collections.Generic;

namespace Disco.Models.Services.Jobs.Exporting
{
    public class JobExportRecord : IExportRecord
    {
        public Job Job { get; set; }
        public string JobStatus { get; set; }
        public string JobTypeDescription { get; set; }
        public IEnumerable<string> JobSubTypeDescriptions { get; set; }

        // Logs
        public int? LogCount { get; set; }
        public JobLog FirstLog { get; set; }
        public JobLog LastLog { get; set; }

        // Attachments
        public int? AttachmentsCount { get; set; }
        public DateTime? AttachmentsLastDate { get; set; }

        // Queues
        public int? QueueCount { get; set; }
        public int? QueueActiveCount { get; set; }
        public JobQueueJob QueueLatestActive { get; set; }

        public JobMetaWarranty JobMetaWarranty { get; set; }
        public JobMetaNonWarranty JobMetaNonWarranty { get; set; }
        public JobMetaInsurance JobMetaInsurance { get; set; }

        // User
        public User User { get; set; }
        public Dictionary<string, string> UserCustomDetails { get; set; }

        // Device
        public Device Device { get; set; }

        // Device Model
        public int? DeviceModelId { get; set; }
        public string DeviceModelDescription { get; set; }
        public string DeviceModelManufacturer { get; set; }
        public string DeviceModelModel { get; set; }
        public string DeviceModelType { get; set; }

        // Device Batch
        public int? DeviceBatchId { get; set; }
        public string DeviceBatchName { get; set; }
        public DateTime? DeviceBatchPurchaseDate { get; set; }
        public string DeviceBatchSupplier { get; set; }
        public decimal? DeviceBatchUnitCost { get; set; }
        public DateTime? DeviceBatchWarrantyValidUntilDate { get; set; }
        public DateTime? DeviceBatchInsuredDate { get; set; }
        public string DeviceBatchInsuranceSupplier { get; set; }
        public DateTime? DeviceBatchInsuredUntilDate { get; set; }

        // Profile
        public int? DeviceProfileId { get; set; }
        public string DeviceProfileName { get; set; }
        public string DeviceProfileShortName { get; set; }
    }
}
