using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Devices.Importing.Fields;
using Disco.Services.Extensions;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Disco.Services.Devices.Importing.Intune
{
    internal class IntuneDeviceImport : IDeviceImportContextActions
    {
        private readonly MemoryStream stream;
        private readonly List<IntuneDeviceImportColumn> columns = new List<IntuneDeviceImportColumn>()
        {
            IntuneDeviceImportColumn.Create<DeviceSerialNumberImportField>(0, "Serial number", DeviceImportFieldTypes.DeviceSerialNumber, r => r.SerialNumber),
            IntuneDeviceImportColumn.Create<IntuneDeviceComputerNameImportField>(1, "Device name", DeviceImportFieldTypes.DeviceComputerName, r => r.DeviceName),
            IntuneDeviceImportColumn.Create<IntuneDeviceCheckInDateImportField>(2, "Last check-in", DeviceImportFieldTypes.IgnoreColumn, r => r.LastCheckIn),
            IntuneDeviceImportColumn.Create<IntuneDeviceModelImportField>(3, "Manufacturer / Model", DeviceImportFieldTypes.ModelId, r => null),
            IntuneDeviceImportColumn.Create<IntuneAssignedUserIdImportField>(4, "Primary user UPN", DeviceImportFieldTypes.AssignedUserId, r => r.PrimaryUserUpn),
            IntuneDeviceImportColumn.Create<DetailWLanMacAddressImportField>(5, "Wi-Fi MAC", DeviceImportFieldTypes.DetailWLanMacAddress, r => r.WiFiMac),
            IntuneDeviceImportColumn.Create<DetailLanMacAddressImportField>(6, "EthernetMAC", DeviceImportFieldTypes.DetailLanMacAddress, r => r.EthernetMac),
        };
        private readonly IntuneDeviceImportConfiguration configuration;

        public string SessionId { get; }
        public string Filename { get; }
        public string DatasetName => Filename;
        public List<IDeviceImportRecord> Records { get; } = new List<IDeviceImportRecord>();
        public List<IDeviceImportColumn> Columns => columns.Cast<IDeviceImportColumn>().ToList();

        public bool AllowBacktracking => false;

        public int RecordCount => Records.Count;
        public int AffectedRecords { get; private set; }


        private IntuneDeviceImport(string filename, Stream stream, IntuneDeviceImportConfiguration configuration)
        {
            SessionId = Guid.NewGuid().ToString("N");
            Filename = filename;

            this.configuration = configuration;
            if (!configuration.AssignDeviceToPrimaryUser)
                columns.RemoveAll(c => c.Type == DeviceImportFieldTypes.AssignedUserId);

            this.stream = new MemoryStream();
            stream.CopyTo(this.stream);
            this.stream.Position = 0;
        }

        public static IntuneDeviceImport Create(string filename, Stream stream, IntuneDeviceImportConfiguration configuration)
            => new IntuneDeviceImport(filename, stream, configuration);

        public void Parse(IScheduledTaskStatus status)
        {
            status.UpdateStatus(5, "Processing import file");

            var csvRecords = ExtractCsv(stream);

            status.UpdateStatus(25, "Validating records");

            var records = new List<(IDeviceImportRecord Record, Device ExistingDevice)>();

            using (var database = DiscoDataContextProvider.Create())
            {
                var state = new DeviceImportDatabaseParsingState(database);

                status.ProgressOffset = 25;
                status.ProgressMultiplier = .7;

                for (int i = 0; i < csvRecords.Count; i++)
                {
                    var csvRecord = csvRecords[i];

                    string deviceSerialNumber = DeviceSerialNumberImportField.ParseRawDeviceSerialNumber(csvRecord.SerialNumber);

                    status.UpdateStatus(i, csvRecords.Count, $"Parsing: {deviceSerialNumber}");

                    Device existingDevice = null;
                    if (DeviceSerialNumberImportField.IsDeviceSerialNumberValid(deviceSerialNumber))
                        existingDevice = state.FindDevice(deviceSerialNumber);

                    var fields = ParseFields(state, existingDevice, csvRecord).ToList();

                    EntityState recordAction;
                    if (fields.Any(f => !f.FieldAction.HasValue))
                        recordAction = EntityState.Detached;
                    else if (existingDevice == null)
                        recordAction = EntityState.Added;
                    else if (fields.Any(f => f.FieldAction == EntityState.Modified))
                        recordAction = EntityState.Modified;
                    else
                        recordAction = EntityState.Unchanged;

                    var record = new IntuneDeviceRecord(deviceSerialNumber, csvRecord, fields, recordAction);
                    records.Add((record, existingDevice));
                }

                status.ProgressMultiplier = 1;
                status.ProgressOffset = 0;

                // Evaluate parsed records for any additional validation that requires knowledge of other records
                foreach (var (record, existingDevice) in records)
                    foreach (var field in record.Fields.Where(f => f.FieldAction.HasValue).Cast<DeviceImportFieldBase>())
                        field.Parsed(state, existingDevice, records.Select(r => r.Record), record);
            }

            Records.AddRange(records.Select(r => r.Record));
        }

        public void Apply(IScheduledTaskStatus status)
        {
            if (Records.Count == 0)
                throw new InvalidOperationException("There are no records to import");

            status.UpdateStatus(0, "Applying Import Records to Database", "Starting...");

            var missingDeviceModels = Records
                .SelectMany(r => r.Fields.OfType<IntuneDeviceModelImportField>())
                .Where(f => f.DeviceModel == null)
                .GroupBy(f => (f.ManufacturerName, f.ModelName))
                .ToList();

            using (var database = DiscoDataContextProvider.Create())
            {
                foreach (var group in missingDeviceModels)
                {
                    var (deviceModel, _) = database.DeviceModels.GetOrCreateDeviceModel(group.Key.ManufacturerName, group.Key.ModelName, "Intune");
                    foreach (var field in group)
                        field.SetDeviceModel(deviceModel);
                }

                int affectedRecords = 0;
                status.ProgressOffset = 10;
                status.ProgressMultiplier = .9;

                foreach (var (record, index) in Records.Select((r, i) => Tuple.Create(r, i)))
                {
                    status.UpdateStatus(index, Records.Count, $"Applying: {record.DeviceSerialNumber}");

                    if (DeviceImportRecord.Apply(record, database, newAllowsUnauthenticatedEnrol: false, defaultDeviceProfileId: configuration.DefaultDeviceProfileId))
                        affectedRecords++;
                }

                status.ProgressMultiplier = 1;
                status.ProgressOffset = 0;
                AffectedRecords = affectedRecords;

                database.DiscoConfiguration.Devices.LastIntuneImportConfiguration = configuration;
                database.SaveChanges();
            }
        }

        private IEnumerable<IDeviceImportField> ParseFields(DeviceImportDatabaseParsingState state, Device existingDevice, IntuneCsvDeviceRecord csvRecord)
        {
            foreach (var column in columns)
            {
                var field = (DeviceImportFieldBase)column.GetHandlerInstance();
                field.Parse(state, existingDevice, IntuneDeviceImportValueReader.CreateValue(csvRecord, column.ValueSelector));
                yield return field;
            }
        }

        private static List<IntuneCsvDeviceRecord> ExtractCsv(MemoryStream stream)
        {
            if (IsZipStream(stream))
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
                {
                    if (archive.Entries.Count != 1)
                        throw new InvalidDataException("Zip import file must contain exactly one CSV file.");

                    var entry = archive.Entries[0];

                    if (!entry.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("Zip import file must contain a single .csv file.");

                    stream = new MemoryStream();
                    using (var entryStream = entry.Open())
                        entryStream.CopyTo(stream);

                    stream.Position = 0;
                }
            }

            return Csv.Parse<IntuneCsvDeviceRecord>(stream).ToList();
        }

        private static bool IsZipStream(Stream stream)
        {
            var originalPosition = stream.Position;
            var header = new byte[4];
            var bytesRead = stream.Read(header, 0, header.Length);
            stream.Position = originalPosition;

            if (bytesRead < 4)
                return false;

            // 'PK' signature
            if (header[0] != 0x50 || header[1] != 0x4B)
                return false;

            // PK.. signatures: local file header, empty archive, spanned archive
            return (header[2] == 0x03 && header[3] == 0x04)
                || (header[2] == 0x05 && header[3] == 0x06)
                || (header[2] == 0x07 && header[3] == 0x08);
        }

        internal sealed class IntuneDeviceImportValueReader : IDeviceImportValueReader
        {
            private readonly object columnValue;

            public IntuneCsvDeviceRecord Record { get; }

            private IntuneDeviceImportValueReader(IntuneCsvDeviceRecord record, Func<IntuneCsvDeviceRecord, object> valueSelector)
            {
                Record = record;
                columnValue = valueSelector(record);
            }

            public static IDeviceImportValueReader CreateValue(IntuneCsvDeviceRecord record, Func<IntuneCsvDeviceRecord, object> valueSelector)
            {
                return new IntuneDeviceImportValueReader(record, valueSelector);
            }

            public string GetString()
            {
                if (columnValue == null)
                    return null;
                else
                    return columnValue.ToString();
            }

            public bool TryGetNullableInt(out int? value)
            {
                if (columnValue == null)
                {
                    value = null;
                    return true;
                }
                else if (columnValue is int intValue)
                {
                    value = intValue;
                    return true;
                }

                throw new InvalidOperationException($"Cannot convert value of type {columnValue.GetType().Name} to int.");
            }

            public bool TryGetNullableBool(out bool? value)
            {
                if (columnValue == null)
                {
                    value = null;
                    return true;
                }
                else if (columnValue is bool boolValue)
                {
                    value = boolValue;
                    return true;
                }

                throw new InvalidOperationException($"Cannot convert value of type {columnValue.GetType().Name} to bool.");
            }

            public bool TryGetNullableDateTime(out DateTime? value)
            {
                if (columnValue == null)
                {
                    value = null;
                    return true;
                }
                else if (columnValue is DateTime dateTimeValue)
                {
                    value = dateTimeValue;
                    return true;
                }

                throw new InvalidOperationException($"Cannot convert value of type {columnValue.GetType().Name} to DateTime.");
            }
        }
    }
}
