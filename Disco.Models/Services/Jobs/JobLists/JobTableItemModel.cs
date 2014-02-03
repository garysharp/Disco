using System;

namespace Disco.Models.Services.Jobs.JobLists
{
    public class JobTableItemModel
    {
        public int Id { get; set; }
        public DateTime OpenedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string JobTypeId { get; set; }
        public string JobTypeDescription { get; set; }
        public string DeviceSerialNumber { get; set; }
        public int? DeviceModelId { get; set; }
        public string DeviceModelDescription { get; set; }
        public int? DeviceProfileId { get; set; }
        public int? DeviceAddressId { get; set; }
        public string DeviceAddress { get; set; }
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public string OpenedTechUserId { get; set; }
        public string OpenedTechUserDisplayName { get; set; }
        public string StatusDescription { get; set; }
        public string StatusId { get; set; }
        public string DeviceHeldLocation { get; set; }
        public Disco.Models.Repository.Job.UserManagementFlags? Flags { get; set; }
    }
}
