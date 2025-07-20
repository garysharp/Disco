using Disco.Models.Services.Searching;
using System;

namespace Disco.Models.Services.Jobs.JobLists
{
    public class JobTableItemModel : JobSearchResultItem
    {
        public int JobId { get; set; }

#pragma warning disable 809
        [Obsolete("Use [int] JobId instead")]
        public override string Id
        {
            get
            {
                return this.JobId.ToString();
            }
            set
            {
                base.Id = value;
                this.JobId = int.Parse(value);
            }
        }
#pragma warning restore 618
        public DateTime OpenedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string JobTypeId { get; set; }
        public string JobTypeDescription { get; set; }
        public int? DeviceModelId { get; set; }
        public string DeviceModelDescription { get; set; }
        public int? DeviceProfileId { get; set; }
        public int? DeviceAddressId { get; set; }
        public string DeviceAddress { get; set; }
        public string OpenedTechUserId { get; set; }
        public string OpenedTechUserFriendlyId { get; set; }
        public string OpenedTechUserDisplayName { get; set; }
        public string StatusDescription { get; set; }
        public string StatusId { get; set; }
        public string DeviceHeldLocation { get; set; }
        public Repository.Job.UserManagementFlags? Flags { get; set; }
        public DateTime LastActivityDate { get; set; }
    }
}
