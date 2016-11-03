using Disco.Data.Repository;
using System;

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

        public UserHeldDeviceModel ToUserHeldDeviceModel(DiscoDataContext Database)
        {
            var uhdm = new UserHeldDeviceModel()
                {
                    UserId = this.UserId,
                    UserDisplayName = this.UserDisplayName,
                    ReadyForReturn = this.ReadyForReturn,
                    WaitingForUserAction = this.WaitingForUserAction,
                    DeviceProfileId = this.DeviceProfileId,
                    DeviceAddress = (this.DeviceAddressId.HasValue ? Database.DiscoConfiguration.OrganisationAddresses.GetAddress(this.DeviceAddressId.Value)?.ShortName : string.Empty)
                };
            var n = DateTime.Now;
            if (!this.ReadyForReturn && this.EstimatedReturnTime.HasValue && this.EstimatedReturnTime.Value > n)
            {
                uhdm.EstimatedReturnTime = this.EstimatedReturnTime.FromNow();
            }
            if (this.ReadyForReturn)
            {
                uhdm.ReadyForReturnSince = this.ReadyForReturnSince.FromNow();
                uhdm.IsAlert = (this.ReadyForReturnSince.Value < DateTime.Now.AddDays(-3));
            }
            if (this.WaitingForUserAction)
            {
                uhdm.WaitingForUserActionSince = this.WaitingForUserActionSince.FromNow();
                uhdm.IsAlert = (this.WaitingForUserActionSince.Value < n.AddDays(-6));
            }
            return uhdm;
        }
    }
}