using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Disco.Web.Areas.Config.Models.DeviceModel
{
    public class _IndexModelDeviceModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string ModelType { get; set; }
        public int DeviceCount { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
                return string.Format("{0} {1}", Manufacturer, Model);
            else
                return Name;
        }
    }
}