using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Models.DeviceBatch
{
    public class ShowModel
    {
        public Disco.Models.Repository.DeviceBatch DeviceBatch { get; set; }
        public List<SelectListItem> DeviceModels { get; set; }
        public int DeviceCount { get; set; }
        public int DeviceDecommissionedCount { get; set; }
        public bool CanDelete { get; set; }
    }
}