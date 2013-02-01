using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Models.BI.Job
{
    public class JobTableModel
    {
        public bool ShowId { get; set; }
        public bool? ShowDeviceAddress { get; set; }
        public bool ShowDates { get; set; }
        public bool ShowType { get; set; }
        public bool ShowDevice { get; set; }
        public bool ShowUser { get; set; }
        public bool ShowTechnician { get; set; }
        public bool ShowLocation { get; set; }
        public bool ShowStatus { get; set; }
        public bool IsSmallTable { get; set; }
        public bool HideClosedJobs { get; set; }
        public List<JobTableItemModel> Items { get; set; }

        public JobTableModel()
        {
            ShowId = true;
            ShowDates = true;
            ShowType = true;
            ShowDevice = true;
            ShowUser = true;
            ShowTechnician = true;
        }

        public class JobTableItemModel
        {
            public int Id { get; set; }
            public int? DeviceAddressId { get; set; }
            public string DeviceAddress { get; set; }
            public DateTime OpenedDate { get; set; }
            public DateTime? ClosedDate { get; set; }
            public string TypeId { get; set; }
            public string TypeDescription { get; set; }
            public string DeviceSerialNumber { get; set; }
            public string DeviceModelDescription { get; set; }
            public string UserId { get; set; }
            public string UserDisplayName { get; set; }
            public string OpenedTechUserId { get; set; }
            public string OpenedTechUserDisplayName { get; set; }
            public string StatusDescription { get; set; }
            public string StatusId { get; set; }
            public string Location { get; set; }
        }

        public class JobTableItemModelIncludeStatus : JobTableItemModel
        {
            public string JobMetaWarranty_ExternalReference { get; set; }
            public DateTime? JobMetaWarranty_ExternalCompletedDate { get; set; }

            public DateTime? JobMetaNonWarranty_RepairerLoggedDate { get; set; }
            public DateTime? JobMetaNonWarranty_RepairerCompletedDate { get; set; }
            public DateTime? JobMetaNonWarranty_AccountingChargeAddedDate { get; set; }
            public DateTime? JobMetaNonWarranty_AccountingChargePaidDate { get; set; }
            public DateTime? JobMetaNonWarranty_AccountingChargeRequiredDate { get; set; }
            public bool? JobMetaNonWarranty_IsInsuranceClaim { get; set; }
            public DateTime? JobMetaInsurance_ClaimFormSentDate { get; set; }

            public DateTime? WaitingForUserAction { get; set; }
            public DateTime? DeviceReadyForReturn { get; set; }
            public DateTime? DeviceHeld { get; set; }
            public DateTime? DeviceReturnedDate { get; set; }
            public string JobMetaWarranty_ExternalName { get; set; }
            public string JobMetaNonWarranty_RepairerName { get; set; }
        }
    }
}
