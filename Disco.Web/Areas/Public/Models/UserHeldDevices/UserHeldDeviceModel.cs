using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.Public.Models.UserHeldDevices
{
    public class UserHeldDeviceModel
    {
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public bool ReadyForReturn { get; set; }
        public string EstimatedReturnTime { get; set; }
        public string ReadyForReturnSince { get; set; }
        public bool IsAlert { get; set; }
        public bool WaitingForUserAction { get; set; }
        public string WaitingForUserActionSince { get; set; }
        public Nullable<DateTime> UpdateAt { get; set; }
        public int DeviceProfileId { get; set; }
        public string DeviceAddress { get; set; }
    }
}