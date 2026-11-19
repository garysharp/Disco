using Disco.Models.Services.Devices.Importing;
using Disco.Models.UI.Device;
using System.Data;
using System.Linq;

namespace Disco.Web.Models.Device
{
    public class ImportVendorModel : DeviceImportIntuneModel, DeviceImportGoogleModel
    {
        public string VendorName { get; }
        public IDeviceImportContext Context { get; }

        public int ErrorRecordCount { get; }
        public int NewRecordCount { get; }
        public int ModifiedRecordCount { get; }
        public int UnmodifiedRecordCount { get; }
        public int ImportRecordCount { get; }

        public ImportVendorModel(string vendorName, IDeviceImportContext context)
        {
            VendorName = vendorName;
            Context = context;

            ErrorRecordCount = context.Records.Count(r => r.HasError);
            NewRecordCount = context.Records.Count(r => r.RecordAction == EntityState.Added);
            ModifiedRecordCount = context.Records.Count(r => r.RecordAction == EntityState.Modified);
            UnmodifiedRecordCount = context.Records.Count(r => r.RecordAction == EntityState.Unchanged);

            ImportRecordCount = NewRecordCount + ModifiedRecordCount;
        }
    }
}
