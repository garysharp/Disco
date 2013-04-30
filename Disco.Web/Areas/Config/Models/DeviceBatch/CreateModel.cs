using Disco.Models.UI.Config.DeviceBatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.Config.Models.DeviceBatch
{
    public class CreateModel : ConfigDeviceBatchCreateModel
    {
        public Disco.Models.Repository.DeviceBatch DeviceBatch { get; set; }
    }
}