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
                UserId = UserId,
                UserDisplayName = UserDisplayName,
                ReadyForReturn = ReadyForReturn,
                WaitingForUserAction = WaitingForUserAction,
                DeviceProfileId = DeviceProfileId,
                DeviceAddress = (DeviceAddressId.HasValue ? Database.DiscoConfiguration.OrganisationAddresses.GetAddress(DeviceAddressId.Value)?.ShortName : string.Empty)
            };
            var n = DateTime.Now;
            if (!ReadyForReturn && EstimatedReturnTime.HasValue && EstimatedReturnTime.Value > n)
            {
                uhdm.EstimatedReturnTime = EstimatedReturnTime.FromNow();
            }
            if (ReadyForReturn)
            {
                uhdm.ReadyForReturnSince = ReadyForReturnSince.FromNow();
                uhdm.IsAlert = (ReadyForReturnSince.Value < DateTime.Now.AddDays(-3));
            }
            if (WaitingForUserAction)
            {
                uhdm.WaitingForUserActionSince = WaitingForUserActionSince.FromNow();
                uhdm.IsAlert = (WaitingForUserActionSince.Value < n.AddDays(-6));
            }
            return uhdm;
        }
    }
}