using Disco.Models.ClientServices.EnrolmentInformation;
using Disco.Models.Repository;
using System;
using System.Collections.Generic;

namespace Disco.Models.Services.Devices.Exporting
{
    public class DeviceExportRecord
    {
        public Device Device { get; set; }

        // Details
        public IList<DeviceDetail> DeviceDetails { get; set; }
        public List<Bios> DeviceDetailBios { get; set; }
        public List<BaseBoard> DeviceDetailBaseBoard { get; set; }
        public List<ComputerSystem> DeviceDetailComputerSystem { get; set; }
        public List<Processor> DeviceDetailProcessors { get; set; }
        public List<PhysicalMemory> DeviceDetailPhysicalMemory { get; set; }
        public List<DiskDrive> DeviceDetailDiskDrives { get; set; }
        public List<NetworkAdapter> DeviceDetailNetworkAdapters { get; set; }
        public List<string> DeviceDetailLanMacAddresses { get; set; }
        public List<string> DeviceDetailWlanMacAddresses { get; set; }
        public List<Battery> DeviceDetailBatteries { get; set; }

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
        public IList<UserDetail> AssignedUserDetails { get; set; }
        public Dictionary<string, string> AssignedUserCustomDetails { get; set; }

        // Jobs
        public int JobsTotalCount { get; set; }
        public int JobsOpenCount { get; set; }

        // Attachments
        public int AttachmentsCount { get; set; }

        // Certificates
        public IEnumerable<DeviceCertificate> DeviceCertificates { get; set; }
    }
}
