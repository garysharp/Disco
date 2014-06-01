using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Devices.Importing.Fields;
using Disco.Services.Tasks;
using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Devices.Importing
{
    public static class DeviceImport
    {

        internal static Lazy<Dictionary<DeviceImportFieldTypes, Type>> FieldHandlers = new Lazy<Dictionary<DeviceImportFieldTypes, Type>>(() =>
        {
            return new Dictionary<DeviceImportFieldTypes, Type>()
            {
                { DeviceImportFieldTypes.DeviceSerialNumber, typeof(DeviceSerialNumberImportField) },
                { DeviceImportFieldTypes.DeviceAssetNumber, typeof(DeviceAssetNumberImportField) },
                { DeviceImportFieldTypes.DeviceLocation, typeof(DeviceLocationImportField) },
                { DeviceImportFieldTypes.DeviceAllowUnauthenticatedEnrol, typeof(DeviceAllowUnauthenticatedEnrolImportField) },
                { DeviceImportFieldTypes.DeviceDecommissionedDate, typeof(DeviceDecommissionedDateImportField) },
                { DeviceImportFieldTypes.DeviceDecommissionedReason, typeof(DeviceDecommissionedReasonImportField) },

                { DeviceImportFieldTypes.DetailLanMacAddress, typeof(DetailLanMacAddressImportField) },
                { DeviceImportFieldTypes.DetailWLanMacAddress, typeof(DetailWLanMacAddressImportField) },
                { DeviceImportFieldTypes.DetailACAdapter, typeof(DetailACAdapterImportField) },
                { DeviceImportFieldTypes.DetailBattery, typeof(DetailBatteryImportField) },

                { DeviceImportFieldTypes.ModelId, typeof(ModelIdImportField) },

                { DeviceImportFieldTypes.BatchId, typeof(BatchIdImportField) },

                { DeviceImportFieldTypes.ProfileId, typeof(ProfileIdImportField) },

                { DeviceImportFieldTypes.AssignedUserId, typeof(AssignedUserIdImportField) }
            };
        });

        public static DeviceImportContext BeginImport(DiscoDataContext Database, string Filename, bool HasHeader, Stream FileContent)
        {
            if (FileContent == null)
                throw new ArgumentNullException("FileContent");

            if (string.IsNullOrWhiteSpace(Filename))
                Filename = "<None Specified>";

            DeviceImportContext context;
            List<Tuple<string, DeviceImportFieldTypes>> header;
            List<string[]> rawData;

            using (TextReader csvTextReader = new StreamReader(FileContent))
            {
                using (CsvReader csvReader = new CsvReader(csvTextReader, HasHeader))
                {
                    csvReader.DefaultParseErrorAction = ParseErrorAction.ThrowException;
                    csvReader.MissingFieldAction = MissingFieldAction.ReplaceByNull;

                    rawData = csvReader.ToList();
                    header = csvReader.GetFieldHeaders().Select(h => Tuple.Create(h, DeviceImportFieldTypes.IgnoreColumn)).ToList();
                }
            }

            context = new DeviceImportContext(Filename, header, rawData);

            context.GuessHeaderTypes(Database);

            return context;
        }

        private static void GuessHeaderTypes(this DeviceImportContext Context, DiscoDataContext Database)
        {
            FieldHandlers.Value.ToList().ForEach(h =>
            {
                var instance = (DeviceImportFieldBase)Activator.CreateInstance(h.Value);
                var column = instance.GuessHeader(Database, Context);
                if (column.HasValue)
                    Context.Header[column.Value] = Tuple.Create(Context.Header[column.Value].Item1, instance.FieldType);
            });
        }

        public static void UpdateHeaderTypes(this DeviceImportContext Context, List<DeviceImportFieldTypes> HeaderTypes)
        {
            if (HeaderTypes == null)
                throw new ArgumentNullException("HeaderTypes");

            if (HeaderTypes.Count != Context.Header.Count)
                throw new ArgumentException("The number of Header Types supplied does not match the number of Headers", "HeaderTypes");

            if (!HeaderTypes.Any(h => h == DeviceImportFieldTypes.DeviceSerialNumber))
                throw new ArgumentException("At least one column must be the Device Serial Number", "HeaderTypes");

            if (HeaderTypes.Where(h => h != DeviceImportFieldTypes.IgnoreColumn).GroupBy(h => h, (k, i) => Tuple.Create(k, i.Count())).Any(g => g.Item2 > 1))
                throw new ArgumentException("Column types can only be specified once for each type", "HeaderTypes");

            Context.Header = Context.Header.Zip(HeaderTypes, (h, ht) => Tuple.Create(h.Item1, ht)).ToList();
        }

        public static void ParseRecords(this DeviceImportContext Context, DiscoDataContext Database, IScheduledTaskStatus Status)
        {
            if (Context.Header == null)
                throw new InvalidOperationException("The Import Context has not been initialized");

            if (Context.Header.Count == 0)
                throw new InvalidOperationException("No Headers were found");

            if (!Context.Header.Any(h => h.Item2 == DeviceImportFieldTypes.DeviceSerialNumber))
                throw new ArgumentException("At least one column must be the Device Serial Number", "Header");

            if (Context.RawData == null || Context.RawData.Count == 0)
                throw new ArgumentException("No data was found in the import file", "RawData");

            IDeviceImportCache cache;
            if (Context.RawData.Count > 20)
                cache = new DeviceImportInMemoryCache(Database);
            else
                cache = new DeviceImportDatabaseCache(Database);

            Context.HeaderDeviceSerialNumberIndex = Context.Header.IndexOf(Context.Header.First(h => h.Item2 == DeviceImportFieldTypes.DeviceSerialNumber));
            Context.ParsedHeaders = Context.Header
                .Select((h, i) => Tuple.Create(h.Item1, h.Item2, i))
                .Where(h => h.Item2 != DeviceImportFieldTypes.IgnoreColumn)
                .Select(h => new Tuple<string, DeviceImportFieldTypes, Func<string[], string>, Type>(h.Item1, h.Item2, (f) => f[h.Item3], DeviceImport.FieldHandlers.Value[h.Item2]))
                .ToList();

            Status.UpdateStatus(0, "Parsing Import Records", "Starting...");

            Context.Records = Context.RawData.Select((d, recordIndex) =>
            {
                string deviceSerialNumber = Fields.DeviceSerialNumberImportField.ParseRawDeviceSerialNumber(d[Context.HeaderDeviceSerialNumberIndex]);

                Status.UpdateStatus(((double)recordIndex / Context.RawData.Count) * 100, string.Format("Parsing: {0}", deviceSerialNumber));

                Device existingDevice = null;
                if (Fields.DeviceSerialNumberImportField.IsDeviceSerialNumberValid(deviceSerialNumber))
                    existingDevice = cache.Devices.FirstOrDefault(device => device.SerialNumber == deviceSerialNumber);

                var values = Context.ParsedHeaders
                    .ToDictionary(k => k.Item2, k => k.Item3(d));

                var fields = Context.ParsedHeaders.Select(h =>
                {
                    var f = (DeviceImportFieldBase)Activator.CreateInstance(h.Item4);
                    f.Parse(Database, cache, Context, recordIndex, deviceSerialNumber, existingDevice, values, h.Item3(d));
                    return f;
                }).ToList();

                EntityState recordAction;
                if (fields.Any(f => !f.FieldAction.HasValue))
                    recordAction = EntityState.Detached;
                else if (existingDevice == null)
                    recordAction = EntityState.Added;
                else if (fields.Any(f => f.FieldAction == EntityState.Modified))
                    recordAction = EntityState.Modified;
                else
                    recordAction = EntityState.Unchanged;

                return new DeviceImportRecord(deviceSerialNumber, fields, recordAction);
            }).Cast<IDeviceImportRecord>().ToList();
        }

        public static int ApplyRecords(this DeviceImportContext Context, DiscoDataContext Database, IScheduledTaskStatus Status)
        {
            if (Context.Records == null)
                throw new InvalidOperationException("Import Records have not been parsed");

            if (Context.Records.Count == 0)
                throw new InvalidOperationException("There are no records to import");

            Status.UpdateStatus(0, "Applying Import Records to Database", "Starting...");

            int affectedRecords = 0;

            foreach (var record in Context.Records.Cast<DeviceImportRecord>().Select((r, i) => Tuple.Create(r, i)))
            {
                Status.UpdateStatus(((double)record.Item2 / Context.Records.Count) * 100, string.Format("Applying: {0}", record.Item1.DeviceSerialNumber));

                if (record.Item1.Apply(Database))
                    affectedRecords++;
            }

            return affectedRecords;
        }
    }
}
