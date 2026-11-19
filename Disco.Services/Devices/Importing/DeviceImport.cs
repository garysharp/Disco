using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Devices.Importing.Decommissioning;
using Disco.Services.Devices.Importing.Google;
using Disco.Services.Devices.Importing.Intune;
using System;
using System.IO;
using System.Web;
using System.Web.Caching;

namespace Disco.Services.Devices.Importing
{
    public static class DeviceImport
    {
        public static IDeviceImportContextActions BeginImport(DiscoDataContext database, string filename, bool hasHeader, Stream fileContent)
        {
            if (fileContent == null)
                throw new ArgumentNullException(nameof(fileContent));

            if (fileContent.Length == 0)
                throw new ArgumentNullException(nameof(fileContent));

            DeviceImportContextBase context;

            if (filename?.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                // Use Xlsx Context
                context = new XlsxDeviceImportContext(filename, hasHeader, fileContent);
            }
            else
            {
                // Use/Default Csv Context
                context = new CsvDeviceImportContext(filename, hasHeader, fileContent);
            }

            context.GuessHeaderTypes();

            StoreContext(context);

            return context;
        }

        public static IDeviceImportContextActions BeginIntuneImport(string filename, Stream stream, IntuneDeviceImportConfiguration configuration)
        {
            var context = IntuneDeviceImport.Create(filename, stream, configuration);
            StoreContext(context);
            return context;
        }

        public static IDeviceImportContextActions BeginGoogleImport(string filename, Stream stream, GoogleDeviceImportConfiguration configuration)
        {
            var context = new GoogleDeviceImport(filename, stream, configuration);
            StoreContext(context);
            return context;
        }

        public static IDeviceImportContextActions BeginDecommissionImport(DiscoDataContext database, DeviceBatch deviceBatch, DecommissionReasons decommissionReason, bool unassignUsers)
        {
            var context = DeviceDecommissionImportContext.Create(database, deviceBatch, decommissionReason, unassignUsers);
            StoreContext(context);
            return context;
        }

        public static IDeviceImportContextActions BeginDecommissionImport(DiscoDataContext database, DeviceProfile deviceProfile, DecommissionReasons decommissionReason, bool unassignUsers)
        {
            var context = DeviceDecommissionImportContext.Create(database, deviceProfile, decommissionReason, unassignUsers);
            StoreContext(context);
            return context;
        }

        public static IDeviceImportContextActions BeginDecommissionImport(DiscoDataContext database, DeviceModel deviceModel, DecommissionReasons decommissionReason, bool unassignUsers)
        {
            var context = DeviceDecommissionImportContext.Create(database, deviceModel, decommissionReason, unassignUsers);
            StoreContext(context);
            return context;
        }

        internal static void StoreContext(IDeviceImportContextActions context)
        {
            string key = $"DeviceImportContext_{context.SessionId}";
            HttpRuntime.Cache.Insert(key, context, null, DateTime.Now.AddMinutes(60), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
        }

        public static IDeviceImportContextActions RetrieveContext(string sessionId, bool remove = false)
        {
            string key = $"DeviceImportContext_{sessionId}";

            var context = HttpRuntime.Cache.Get(key) as IDeviceImportContextActions;

            if (context != null && remove)
                HttpRuntime.Cache.Remove(key);

            return context;
        }
    }
}
