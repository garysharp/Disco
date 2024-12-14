using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class JobMetaNonWarranty
    {
        [Key, Required]
        public int JobId { get; set; }

        public bool IsInsuranceClaim { get; set; }

        // Feature Request 2012-05-10 by Michael E: https://disco.uservoice.com/forums/159707-feedback/suggestions/2811092-document-template-option-flatten-form-on-generate
        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd hh:mm tt}", HtmlEncode = false)]
        public DateTime? AccountingChargeRequiredDate { get; set; }
        [ForeignKey("AccountingChargeRequiredUserId")]
        public virtual User AccountingChargeRequiredUser { get; set; }
        public string AccountingChargeRequiredUserId { get; set; }
        // End Feature Request

        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd hh:mm tt}", HtmlEncode = false)]
        public DateTime? AccountingChargeAddedDate { get; set; }
        [ForeignKey("AccountingChargeAddedUserId")]
        public virtual User AccountingChargeAddedUser { get; set; }
        public string AccountingChargeAddedUserId { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd hh:mm tt}", HtmlEncode = false)]
        public DateTime? AccountingChargePaidDate { get; set; }
        [ForeignKey("AccountingChargePaidUserId")]
        public virtual User AccountingChargePaidUser { get; set; }
        public string AccountingChargePaidUserId { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd hh:mm tt}", HtmlEncode = false)]
        public DateTime? PurchaseOrderRaisedDate { get; set; }
        [ForeignKey("PurchaseOrderRaisedUserId")]
        public virtual User PurchaseOrderRaisedUser { get; set; }
        public string PurchaseOrderRaisedUserId { get; set; }
        [StringLength(20)]
        public string PurchaseOrderReference { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd hh:mm tt}", HtmlEncode = false)]
        public DateTime? PurchaseOrderSentDate { get; set; }
        [ForeignKey("PurchaseOrderSentUserId")]
        public virtual User PurchaseOrderSentUser { get; set; }
        public string PurchaseOrderSentUserId { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd hh:mm tt}", HtmlEncode = false)]
        public DateTime? InvoiceReceivedDate { get; set; }
        [ForeignKey("InvoiceReceivedUserId")]
        public virtual User InvoiceReceivedUser { get; set; }
        public string InvoiceReceivedUserId { get; set; }

        [StringLength(100)]
        public string RepairerName { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd hh:mm tt}", HtmlEncode = false)]
        public DateTime? RepairerLoggedDate { get; set; }
        [StringLength(100)]
        public string RepairerReference { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd hh:mm tt}", HtmlEncode = false)]
        public DateTime? RepairerCompletedDate { get; set; }

        [ForeignKey("JobId"), Required]
        public virtual Job Job { get; set; }
    }
}
