using System;
using System.Collections.Generic;

namespace Disco.Models.Services.Jobs.JobLists
{
    public class JobTableStatusItemModel : JobTableItemModel
    {
        public string JobMetaWarranty_ExternalReference { get; set; }
        public DateTime? JobMetaWarranty_ExternalLoggedDate { get; set; }
        public DateTime? JobMetaWarranty_ExternalCompletedDate { get; set; }

        public DateTime? JobMetaNonWarranty_RepairerLoggedDate { get; set; }
        public DateTime? JobMetaNonWarranty_RepairerCompletedDate { get; set; }
        public DateTime? JobMetaNonWarranty_AccountingChargeAddedDate { get; set; }
        public DateTime? JobMetaNonWarranty_AccountingChargePaidDate { get; set; }
        public DateTime? JobMetaNonWarranty_AccountingChargeRequiredDate { get; set; }
        public DateTime? JobMetaNonWarranty_PurchaseOrderRaisedDate { get; set; }
        public DateTime? JobMetaNonWarranty_PurchaseOrderSentDate { get; set; }
        public DateTime? JobMetaNonWarranty_InvoiceReceivedDate { get; set; }
        public bool? JobMetaNonWarranty_IsInsuranceClaim { get; set; }
        public DateTime? JobMetaInsurance_ClaimFormSentDate { get; set; }

        public DateTime? WaitingForUserAction { get; set; }
        public DateTime? DeviceReadyForReturn { get; set; }
        public DateTime? DeviceHeld { get; set; }
        public DateTime? DeviceReturnedDate { get; set; }
        public string JobMetaWarranty_ExternalName { get; set; }
        public string JobMetaNonWarranty_RepairerName { get; set; }

        public IEnumerable<JobTableStatusQueueItemModel> ActiveJobQueues { get; set; }

        public DateTime? RecentLogDate { get; set; }
        public DateTime? RecentAttachmentDate { get; set; }
    }
}
