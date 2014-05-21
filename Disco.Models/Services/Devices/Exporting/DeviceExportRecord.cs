using Disco.Models.Repository;
using System;
using System.Collections.Generic;

namespace Disco.Models.Services.Devices.Exporting
{
    public class DeviceExportRecord
    {
        public Device Device { get; set; }

        // Details
        public IEnumerable<DeviceDetail> DeviceDetails { get; set; }
        
        // Model
        public int? ModelId { get; set; }
        public string ModelDescription { get; set; }
        public string ModelManufacturer { get; set; }
        public string ModelModel { get; set; }
        public string ModelType { get; set; }

        // Batch
        public int? BatchId { get; set; }
        public string BatchName { get; set; }
        public DateTime? BatchPurchaseDate { get; set; }
        public string BatchSupplier { get; set; }
        public decimal? BatchUnitCost { get; set; }
        public DateTime? BatchWarrantyValidUntilDate { get; set; }
        public DateTime? BatchInsuredDate { get; set; }
        public string BatchInsuranceSupplier { get; set; }
        public DateTime? BatchInsuredUntilDate { get; set; }

        // Profile
        public int ProfileId { get; set; }
        public string ProfileName { get; set; }
        public string ProfileShortName { get; set; }

        // User
        public DeviceUserAssignment DeviceUserAssignment { get; set; }
        public User AssignedUser { get; set; }

        // Jobs
        public int JobsTotalCount { get; set; }
        public int JobsOpenCount { get; set; }

        // Attachments
        public int AttachmentsCount { get; set; }

        // Certificates
        public DeviceCertificate DeviceCertificate { get; set; }
    }
}
