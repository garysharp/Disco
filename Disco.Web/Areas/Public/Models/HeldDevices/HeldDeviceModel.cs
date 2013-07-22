using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Disco.Web.Areas.Public.Models.HeldDevices
{
    public class HeldDeviceModel
    {
        public string DeviceSerialNumber { get; set; }
        public string DeviceComputerName { get; set; }
        public int DeviceProfileId { get; set; }
        public string DeviceAddress { get; set; }
        public string DeviceLocation { get; set; }
        public string DeviceDescription
        {
            get
            {
                StringBuilder sb = new StringBuilder(this.DeviceComputerName);

                if (UserId != null)
                    sb.Append(" - ").Append(this.UserDisplayName).Append(" (").Append(this.UserId).Append(")");

                if (!string.IsNullOrWhiteSpace(this.DeviceLocation))
                    sb.Append(" - ").Append(this.DeviceLocation);
                else if (UserId == null)
                    sb.Append(" - ").Append(this.DeviceSerialNumber);

                return sb.ToString();
            }
        }

        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public bool WaitingForUserAction { get; set; }
        public string WaitingForUserActionSince { get; set; }

        public bool ReadyForReturn { get; set; }
        public string EstimatedReturnTime { get; set; }
        public string ReadyForReturnSince { get; set; }
        public bool IsAlert { get; set; }
    }
}