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
        public bool EnablePaging { get; set; }
        public bool EnableFilter { get; set; }
        public virtual List<JobTableItemModel> Items { get; set; }

        public JobTableModel()
        {
            ShowId = true;
            ShowDates = true;
            ShowType = true;
            ShowDevice = true;
            ShowUser = true;
            ShowTechnician = true;
            EnablePaging = true;
            EnableFilter = true;
        }

        private JobTableModel CloneEmptyModel()
        {
            return new JobTableModel()
            {
                ShowId = this.ShowId,
                ShowDeviceAddress = this.ShowDeviceAddress,
                ShowDates = this.ShowDates,
                ShowType = this.ShowType,
                ShowDevice = this.ShowDevice,
                ShowUser = this.ShowUser,
                ShowTechnician = this.ShowTechnician,
                ShowLocation = this.ShowLocation,
                ShowStatus = this.ShowStatus,
                IsSmallTable = this.IsSmallTable,
                HideClosedJobs = this.HideClosedJobs,
                EnablePaging = this.EnablePaging,
                EnableFilter = this.EnableFilter
            };
        }

        public IDictionary<string, JobTableModel> MultiCampusModels
        {
            get
            {
                var items = this.Items;
                if (items == null || items.Count > 0)
                {
                    return items.OrderBy(i => i.DeviceAddress).GroupBy(i => i.DeviceAddress).ToDictionary(
                        ig => ig.Key ?? string.Empty,
                        ig =>
                        {
                            var jtm = this.CloneEmptyModel();
                            jtm.Items = ig.ToList();
                            return jtm;
                        }
                    );
                }
                else
                {
                    return null;
                }
            }
        }

        public class JobTableItemModel
        {
            public int Id { get; set; }
            public DateTime OpenedDate { get; set; }
            public DateTime? ClosedDate { get; set; }
            public string TypeId { get; set; }
            public string TypeDescription { get; set; }
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
