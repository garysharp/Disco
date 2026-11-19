using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Models.UI.Device;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Disco.Web.Models.Device
{
    public class ImportModel : DeviceImportModel
    {
        public IDeviceImportContext CompletedImportSessionContext { get; set; }

        public List<DeviceModel> DeviceModels { get; set; }
        public List<DeviceProfile> DeviceProfiles { get; set; }
        public List<DeviceBatch> DeviceBatches { get; set; }

        public IEnumerable<Tuple<string, string, string>> HeaderTypes { get; set; }

        public IntuneDeviceImportConfiguration LastIntuneDeviceImportConfiguration { get; set; }
        public GoogleDeviceImportConfiguration LastGoogleDeviceImportConfiguration { get; set; }

        public ImportModel()
        {
            HeaderTypes = typeof(DeviceImportFieldTypes)
                .GetFields()
                .Select(p => Tuple.Create(p, (DisplayAttribute)p.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault()))
                .Where(p => p.Item2 != null && p.Item1.Name != DeviceImportFieldTypes.IgnoreColumn.ToString())
                .Select(p => Tuple.Create(p.Item1.Name, p.Item2.Name, p.Item2.Description)).ToList();
        }
    }
}