using Disco.Models.Services.Devices.Importing;
using Disco.Models.UI.Device;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Disco.Web.Models.Device
{
    public class ImportReviewModel : DeviceImportReviewModel
    {
        public IDeviceImportContext Context { get; set; }

        public int StatisticErrorRecords { get; set; }
        public int StatisticNewRecords { get; set; }
        public int StatisticModifiedRecords { get; set; }
        public int StatisticUnmodifiedRecords { get; set; }

        public int StatisticImportRecords
        {
            get { return StatisticNewRecords + StatisticModifiedRecords; }
        }

        public IEnumerable<Tuple<DeviceImportFieldTypes, string>> HeaderTypes { get; set; }

        public ImportReviewModel()
        {
            HeaderTypes = typeof(DeviceImportFieldTypes)
                .GetFields()
                .Select(p => Tuple.Create(p, (DisplayAttribute)p.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault()))
                .Where(p => p.Item2 != null)
                .Select(p => Tuple.Create((DeviceImportFieldTypes)p.Item1.GetRawConstantValue(), p.Item2.Name)).ToList();
        }
    }
}