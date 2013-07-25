using Disco.Models.UI.Config.DeviceBatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.Config.Models.DeviceBatch
{
    public class _ShowModelMembership : ConfigDeviceBatchShowModelMembership
    {
        public Disco.Models.Repository.DeviceModel DeviceModel { get; set; }
        public int DeviceCount { get; set; }
        public int DeviceDecommissionedCount { get; set; }
    }
}