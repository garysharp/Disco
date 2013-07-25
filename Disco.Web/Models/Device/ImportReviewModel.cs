using Disco.Models.BI.Device;
using Disco.Models.UI.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Models.Device
{
    public class ImportReviewModel : DeviceImportReviewModel
    {
        public string ImportParseTaskId { get; set; }
        public string ImportFilename { get; set; }
        public List<ImportDevice> ImportDevices { get; set; }

        public static ImportReviewModel FromImportDeviceSession(ImportDeviceSession session)
        {
            return new ImportReviewModel()
            {
                ImportParseTaskId = session.ImportParseTaskId,
                ImportFilename = session.ImportFilename,
                ImportDevices = session.ImportDevices
            };
        }
    }
}