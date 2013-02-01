using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.BI;
using Disco.BI.Extensions;
using Disco.Data.Repository;

namespace Disco.Web.Areas.Public.Models.UserHeldDevices
{
    public class HeldJobDeviceModel
    {
        public int JobId { get; set; }

        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public int DeviceProfileId { get; set; }
        public int? DeviceAddressId { get; set; }
        public bool ReadyForReturn { get; set; }
        public bool WaitingForUserAction { get; set; }
        public DateTime? EstimatedReturnTime { get; set; }
        public DateTime? ReadyForReturnSince { get; set; }
        public DateTime? WaitingForUserActionSince { get; set; }

        public UserHeldDeviceModel ToUserHeldDeviceModel(DiscoDataContext dbContext)
        {
            var uhdm = new UserHeldDeviceModel()
                {
                    UserId = this.UserId,
                    UserDisplayName = this.UserDisplayName,
                    ReadyForReturn = this.ReadyForReturn,
                    WaitingForUserAction = this.WaitingForUserAction,
                    DeviceProfileId = this.DeviceProfileId,
                    DeviceAddress = (this.DeviceAddressId.HasValue ? dbContext.DiscoConfiguration.OrganisationAddresses.GetAddress(this.DeviceAddressId.Value).ShortName : string.Empty)
                };
            var n = DateTime.Now;
            if (!this.ReadyForReturn && this.EstimatedReturnTime.HasValue && this.EstimatedReturnTime.Value > n)
            {
                uhdm.EstimatedReturnTime = this.EstimatedReturnTime.ToFuzzy();
                if (this.EstimatedReturnTime.Value.Date == n.Date)
                {
                    if (this.EstimatedReturnTime.Value < n.AddHours(2))
                    {
                        if (this.EstimatedReturnTime.Value < n.AddMinutes(12))
                        {
                            uhdm.UpdateAt = this.EstimatedReturnTime.Value;
                        }
                        else
                        {
                            uhdm.UpdateAt = this.EstimatedReturnTime.Value.AddMinutes(-10);
                        }
                    }
                }
            }
            if (this.ReadyForReturn)
            {
                uhdm.ReadyForReturnSince = this.ReadyForReturnSince.ToFuzzy();
                uhdm.IsAlert = (this.ReadyForReturnSince.Value < DateTime.Now.AddDays(-3));
            }
            if (this.WaitingForUserAction)
            {
                uhdm.WaitingForUserActionSince = this.WaitingForUserActionSince.ToFuzzy();
                uhdm.IsAlert = (this.WaitingForUserActionSince.Value < n.AddDays(-6));
            }
            return uhdm;
        }
    }
}