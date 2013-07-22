using Disco.Models.UI.Device;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Disco.Web.Models.Device
{
    public class ImportModel : DeviceImportModel
    {
        [Required, Display(Name="Import File")]
        public HttpPostedFileBase ImportFile { get; set; }
    }
}