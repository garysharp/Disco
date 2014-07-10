using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization.Roles.ClaimGroups.Job
{
    [ClaimDetails("Non Warranty Properties", "Permissions related to Non-Warranty Job Properties")]
    public class JobNonWarrantyPropertiesClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Add Components", "Can add job components (NOTE: Requires Edit Components)")]
        public bool AddComponents { get; set; }
        [ClaimDetails("Edit Components", "Can edit and remove job components")]
        public bool EditComponents { get; set; }

        [ClaimDetails("Is Insurance Claim  Property", "Can update property")]
        public bool IsInsuranceClaim { get; set; }

        [ClaimDetails("Insurance Claim Form Sent Property", "Can update property")]
        public bool InsuranceClaimFormSent { get; set; }

        [ClaimDetails("Accounting Charge Required Property", "Can update property")]
        public bool AccountingChargeRequired { get; set; }
        [ClaimDetails("Accounting Charge Added Property", "Can update property")]
        public bool AccountingChargeAdded { get; set; }
        [ClaimDetails("Accounting Charge Paid Property", "Can update property")]
        public bool AccountingChargePaid { get; set; }
        [ClaimDetails("Purchase Order Raised Property", "Can update property")]
        public bool PurchaseOrderRaised { get; set; }
        [ClaimDetails("Purchase Order Reference Property", "Can update property")]
        public bool PurchaseOrderReference { get; set; }
        [ClaimDetails("Purchase Order Sent Property", "Can update property")]
        public bool PurchaseOrderSent { get; set; }
        [ClaimDetails("Invoice Received Property", "Can update property")]
        public bool InvoiceReceived { get; set; }

        [ClaimDetails("Repairer Name Property", "Can update property")]
        public bool RepairerName { get; set; }
        [ClaimDetails("Repairer Completed Date Property", "Can update property")]
        public bool RepairerCompletedDate { get; set; }
        [ClaimDetails("Repairer Logged Date Property", "Can update property")]
        public bool RepairerLoggedDate { get; set; }
        [ClaimDetails("Repairer Reference Property", "Can update property")]
        public bool RepairerReference { get; set; }

        [ClaimDetails("Repair Provider Details", "Can access repair provider details")]
        public bool RepairProviderDetails { get; set; }

        [ClaimDetails("Insurance Detail Properties", "Can update insurance detail properties")]
        public bool InsuranceDetails { get; set; }
    }
}
