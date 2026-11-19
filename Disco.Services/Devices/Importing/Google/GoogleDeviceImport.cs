using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Devices.Importing.Fields;
using Disco.Services.Devices.Importing.Intune;
using Disco.Services.Extensions;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Disco.Services.Devices.Importing.Google
{
    internal class GoogleDeviceImport : IDeviceImportContextActions
    {
        private readonly MemoryStream stream;
        private readonly List<GoogleDeviceImportColumn> columns;
        private readonly GoogleDeviceImportConfiguration configuration;

        public string SessionId { get; }
        public string Filename { get; }
        public string DatasetName => Filename;
        public List<IDeviceImportRecord> Records { get; } = new List<IDeviceImportRecord>();
        public List<IDeviceImportColumn> Columns => columns.Cast<IDeviceImportColumn>().ToList();

        public bool AllowBacktracking => false;

        public int RecordCount => Records.Count;
        public int AffectedRecords { get; private set; }

        internal GoogleDeviceImport(string filename, Stream stream, GoogleDeviceImportConfiguration configuration)
        {
            SessionId = Guid.NewGuid().ToString("N");
            Filename = filename;

            this.configuration = configuration;

            var columnNumber = 0;

            columns = new List<GoogleDeviceImportColumn>()
            {
                GoogleDeviceImportColumn.Create<DeviceSerialNumberImportField>(columnNumber++, "serialNumber", DeviceImportFieldTypes.DeviceSerialNumber, r => r.SerialNumber),
                GoogleDeviceImportColumn.Create<GoogleDeviceModelImportField>(columnNumber++, "model", DeviceImportFieldTypes.ModelId, r => r.Model),
                GoogleDeviceImportColumn.Create<IntuneDeviceCheckInDateImportField>(columnNumber++, "lastPolicySync", DeviceImportFieldTypes.IgnoreColumn, r => r.LastPolicySync),
            };
            if (configuration.AssignedUserSource != GoogleDeviceImportUserSource.None)
            {
                string name;
                Func<GoogleCsvDeviceRecord, object> valueSelector;
                if (configuration.AssignedUserSource == GoogleDeviceImportUserSource.AnnotatedUser)
                {
                    name = "annotatedUser";
                    valueSelector = r => r.AnnotatedUser;
                }
                else if (configuration.AssignedUserSource == GoogleDeviceImportUserSource.MostRecentUser)
                {
                    name = "mostRecentUser";
                    valueSelector = r => r.MostRecentUser;
                }
                else
                    throw new InvalidOperationException($"Unknown AssignedUserSource: {configuration.AssignedUserSource}");

                columns.Add(GoogleDeviceImportColumn.Create<IntuneAssignedUserIdImportField>(columnNumber++, name, DeviceImportFieldTypes.AssignedUserId, valueSelector));
            }

            columns.Add(GoogleDeviceImportColumn.Create<DeviceLocationImportField>(columnNumber++, "annotatedLocation", DeviceImportFieldTypes.DeviceLocation, r => r.AnnotatedLocation));
            columns.Add(GoogleDeviceImportColumn.Create<DetailLanMacAddressImportField>(columnNumber++, "ethernetMacAddress", DeviceImportFieldTypes.DetailLanMacAddress, r => r.EthernetMacAddress));
            columns.Add(GoogleDeviceImportColumn.Create<DetailWLanMacAddressImportField>(columnNumber, "macAddress", DeviceImportFieldTypes.DetailWLanMacAddress, r => r.MacAddress));

            this.stream = new MemoryStream();
            stream.CopyTo(this.stream);
            this.stream.Position = 0;
        }

        public void Parse(IScheduledTaskStatus status)
        {
            status.UpdateStatus(5, "Processing import file");

            var csvRecords = Csv.Parse<GoogleCsvDeviceRecord>(stream).ToList();
            if (csvRecords.Count == 0)
                throw new InvalidDataException("No devices were found in the Google Admin export.");

            status.UpdateStatus(25, "Validating records");

            var records = new List<(GoogleDeviceRecord Record, Device ExistingDevice)>();

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

                    var record = new GoogleDeviceRecord(deviceSerialNumber, csvRecord, fields, recordAction);
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

        private IEnumerable<IDeviceImportField> ParseFields(DeviceImportDatabaseParsingState state, Device existingDevice, GoogleCsvDeviceRecord csvRecord)
        {
            foreach (var column in columns)
            {
                var field = (DeviceImportFieldBase)column.GetHandlerInstance();
                field.Parse(state, existingDevice, GoogleDeviceImportValueReader.CreateValue(csvRecord, column.ValueSelector));
                yield return field;
            }
        }

        public void Apply(IScheduledTaskStatus status)
        {
            if (Records.Count == 0)
                throw new InvalidOperationException("There are no records to import");

            status.UpdateStatus(0, "Applying Import Records to Database", "Starting...");

            var missingDeviceModels = Records
                .SelectMany(r => r.Fields.OfType<GoogleDeviceModelImportField>())
                .Where(f => f.DeviceModel == null && !string.IsNullOrWhiteSpace(f.ModelName))
                .GroupBy(f => f.ModelName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            using (var database = DiscoDataContextProvider.Create())
            {
                foreach (var group in missingDeviceModels)
                {
                    // Assume the first word is the manufacturer and the rest is the model name. If there is only one word, assume the manufacturer is unknown.
                    var firstSpaceIndex = group.Key.IndexOf(' ');
                    var manufacturerName = firstSpaceIndex > 0 ? group.Key.Substring(0, firstSpaceIndex) : "Unknown";
                    var modelName = firstSpaceIndex > 0 ? group.Key.Substring(firstSpaceIndex + 1) : group.Key;

                    var (deviceModel, _) = database.DeviceModels.GetOrCreateDeviceModel(manufacturerName, modelName, "Google");
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

                database.DiscoConfiguration.Devices.LastGoogleImportConfiguration = configuration;
                database.SaveChanges();
            }
        }

        private sealed class GoogleDeviceImportValueReader : IDeviceImportValueReader
        {
            private readonly object columnValue;

            public GoogleCsvDeviceRecord Record { get; }

            private GoogleDeviceImportValueReader(GoogleCsvDeviceRecord record, Func<GoogleCsvDeviceRecord, object> valueSelector)
            {
                Record = record;
                columnValue = valueSelector(record);
            }

            public static IDeviceImportValueReader CreateValue(GoogleCsvDeviceRecord record, Func<GoogleCsvDeviceRecord, object> valueSelector)
            {
                return new GoogleDeviceImportValueReader(record, valueSelector);
            }

            public string GetString()
            {
                if (columnValue == null)
                    return null;
                else
                    return columnValue.ToString();
            }

            public bool TryGetNullableInt(out int? value)
                => throw new NotImplementedException();

            public bool TryGetNullableBool(out bool? value)
                => throw new NotImplementedException();

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
