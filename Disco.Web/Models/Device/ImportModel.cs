using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
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
        [Required, Display(Name = "CSV Import File")]
        public HttpPostedFileBase ImportFile { get; set; }

        [Required, Display(Name = "Has Header")]
        public bool HasHeader { get; set; }

        public IDeviceImportContext CompletedImportSessionContext { get; set; }

        public List<DeviceModel> DeviceModels { get; set; }
        public List<DeviceProfile> DeviceProfiles { get; set; }
        public List<DeviceBatch> DeviceBatches { get; set; }

        public IEnumerable<Tuple<string, string, string>> HeaderTypes { get; set; }

        public ImportModel()
        {
            HasHeader = true;

            HeaderTypes = typeof(DeviceImportFieldTypes)
                .GetFields()
                .Select(p => Tuple.Create(p, (DisplayAttribute)p.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault()))
                .Where(p => p.Item2 != null && p.Item1.Name != DeviceImportFieldTypes.IgnoreColumn.ToString())
                .Select(p => Tuple.Create(p.Item1.Name, p.Item2.Name, p.Item2.Description)).ToList();
        }
    }
}