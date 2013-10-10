using Disco.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.BI.Extensions;

namespace Disco.Web.Areas.Public.Models.HeldDevices
{
    public class HeldDeviceQueryModel
    {
        public int JobId { get; set; }

        public string DeviceSerialNumber { get; set; }
        public string DeviceComputerName { get; set; }
        public int DeviceProfileId { get; set; }
        public int? DeviceAddressId { get; set; }
        public string DeviceLocation { get; set; }

        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        
        public bool ReadyForReturn { get; set; }
        public bool WaitingForUserAction { get; set; }
        public DateTime? EstimatedReturnTime { get; set; }
        public DateTime? ReadyForReturnSince { get; set; }
        public DateTime? WaitingForUserActionSince { get; set; }

        public HeldDeviceModel ToUserHeldDeviceModel(DiscoDataContext Database)
        {
            var uhdm = new HeldDeviceModel()
            {
                DeviceSerialNumber = this.DeviceSerialNumber,
                DeviceComputerName = this.DeviceComputerName,
                DeviceLocation = this.DeviceLocation,
                DeviceProfileId = this.DeviceProfileId,
                DeviceAddress = (this.DeviceAddressId.HasValue ? Database.DiscoConfiguration.OrganisationAddresses.GetAddress(this.DeviceAddressId.Value).ShortName : string.Empty),
                UserId = this.UserId,
                UserDisplayName = this.UserDisplayName,
                ReadyForReturn = this.ReadyForReturn,
                WaitingForUserAction = this.WaitingForUserAction
            };
            var n = DateTime.Now;
            if (!this.ReadyForReturn && this.EstimatedReturnTime.HasValue && this.EstimatedReturnTime.Value > n)
            {
                uhdm.EstimatedReturnTime = this.EstimatedReturnTime.ToFuzzy();
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