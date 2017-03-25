using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Devices.Importing.Fields;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Disco.Services.Devices.Importing
{
    public static class DeviceImport
    {

        public static IDeviceImportContext BeginImport(DiscoDataContext Database, string Filename, bool HasHeader, Stream FileContent)
        {
            if (FileContent == null)
                throw new ArgumentNullException(nameof(FileContent));

            if (FileContent.Length == 0)
                throw new ArgumentNullException(nameof(FileContent));

            IDeviceImportContext context;

            if (Filename?.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                // Use Xlsx Context
                context = new XlsxDeviceImportContext(Filename, HasHeader, FileContent);
            }
            else
            {
                // Use/Default Csv Context
                context = new CsvDeviceImportContext(Filename, HasHeader, FileContent);
            }

            context.GuessHeaderTypes(Database);

            return context;
        }

        private static void GuessHeaderTypes(this IDeviceImportContext Context, DiscoDataContext Database)
        {
            using (var dataReader = Context.GetDataReader())
            {
                foreach (var fieldHandler in Context.GetFieldHandlers())
                {
                    dataReader.Reset();

                    var instance = (DeviceImportFieldBase)Activator.CreateInstance(fieldHandler.Value);
                    var column = instance.GuessColumn(Database, Context, dataReader);

                    if (column.HasValue)
                        Context.SetColumnType(column.Value, instance.FieldType);
                }
            }
        }

        public static void UpdateColumnTypes(this IDeviceImportContext Context, List<DeviceImportFieldTypes> ColumnTypes)
        {
            if (ColumnTypes == null)
                throw new ArgumentNullException(nameof(ColumnTypes));

            if (ColumnTypes.Count != Context.ColumnCount)
                throw new ArgumentException("The number of Column Types supplied does not match the number of Headers", nameof(ColumnTypes));

            if (!ColumnTypes.Any(h => h == DeviceImportFieldTypes.DeviceSerialNumber))
                throw new ArgumentException("At least one column must be the Device Serial Number", nameof(ColumnTypes));

            if (ColumnTypes.Where(h => h != DeviceImportFieldTypes.IgnoreColumn).GroupBy(h => h, (k, i) => Tuple.Create(k, i.Count())).Any(g => g.Item2 > 1))
                throw new ArgumentException("Column types can only be specified once for each type", nameof(ColumnTypes));

            for (int columnIndex = 0; columnIndex < Context.ColumnCount; columnIndex++)
            {
                Context.SetColumnType(columnIndex, ColumnTypes[columnIndex]);
            }
        }

        public static void ParseRecords(this IDeviceImportContext Context, DiscoDataContext Database, IScheduledTaskStatus Status)
        {
            if (Context.ColumnCount == 0)
                throw new InvalidOperationException("No columns were found");

            if (!Context.GetColumnByType(DeviceImportFieldTypes.DeviceSerialNumber).HasValue)
                throw new ArgumentException("At least one column must be the Device Serial Number", nameof(Context.Columns));

            if (Context.RecordCount == 0)
                throw new ArgumentException("No data was found in the import file", nameof(Context.RecordCount));

            IDeviceImportCache cache;
            if (Context.RecordCount > 20)
                cache = new DeviceImportInMemoryCache(Database);
            else
                cache = new DeviceImportDatabaseCache(Database);

            var deviceSerialNumberIndex = Context.GetColumnByType(DeviceImportFieldTypes.DeviceSerialNumber).Value;
            var columns = Context.Columns
                .Where(h => h.Type != DeviceImportFieldTypes.IgnoreColumn)
                .ToList();

            Status.UpdateStatus(0, "Parsing Import Records", "Starting...");

            var records = new List<IDeviceImportRecord>();

            using (var dataReader = Context.GetDataReader())
            {
                while (dataReader.Read())
                {
                    string deviceSerialNumber = DeviceSerialNumberImportField.ParseRawDeviceSerialNumber(dataReader.GetString(deviceSerialNumberIndex));

                    Status.UpdateStatus(((double)dataReader.Index / Context.RecordCount) * 100, string.Format("Parsing: {0}", deviceSerialNumber));

                    Device existingDevice = null;
                    if (DeviceSerialNumberImportField.IsDeviceSerialNumberValid(deviceSerialNumber))
                        existingDevice = cache.Devices.FirstOrDefault(device => device.SerialNumber == deviceSerialNumber);

                    var fields = columns.Select(h =>
                    {
                        var f = (DeviceImportFieldBase)h.GetHandlerInstance();
                        f.Parse(Database, cache, Context, deviceSerialNumber, existingDevice, records, dataReader, h.Index);
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

                    records.Add(new DeviceImportRecord(dataReader.Index, deviceSerialNumber, fields, recordAction));
                }
            }

            Context.Records = records;
        }

        public static int ApplyRecords(this IDeviceImportContext Context, DiscoDataContext Database, IScheduledTaskStatus Status)
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
