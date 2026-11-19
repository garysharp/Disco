using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Devices.Importing.Fields;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing
{
    public abstract class DeviceImportContextBase : IDeviceImportContextActions
    {
        protected List<DeviceImportColumn> columns;
        private Dictionary<DeviceImportFieldTypes, DeviceImportColumn> columnsByType;

        public string SessionId { get; }
        public string Filename { get; }
        public string DatasetName { get; private set; }

        public abstract int RecordCount { get; }

        public List<IDeviceImportRecord> Records { get; protected set; }
        public int AffectedRecords { get; private set; }

        public bool AllowBacktracking { get; protected set; } = true;

        public int ColumnCount { get { return columns.Count; } }
        public List<IDeviceImportColumn> Columns
        {
            get
            {
                return columns?.Cast<IDeviceImportColumn>().ToList() ?? throw new InvalidOperationException("Columns have not been set. Call SetColumns() before accessing Columns.");
            }
        }

        protected DeviceImportContextBase(string Filename)
        {
            SessionId = Guid.NewGuid().ToString("D");
            this.Filename = DatasetName = string.IsNullOrWhiteSpace(Filename) ? "<File Name Not Specified>" : Filename;
        }

        public abstract IDeviceImportDataReader GetDataReader();

        public IDeviceImportColumn GetColumn(int Index)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return columns[Index];
        }

        private void SetColumnType(int Index, DeviceImportFieldTypes Type)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            var column = columns[Index];

            if (column.Type == Type)
            {
                return; // No change
            }

            if (column.Type != DeviceImportFieldTypes.IgnoreColumn)
            {
                columnsByType.Remove(column.Type);
            }

            column.Type = Type;

            if (Type == DeviceImportFieldTypes.IgnoreColumn)
            {
                column.Handler = null;
            }
            else
            {
                columnsByType[Type] = column;
                column.Handler = DeviceImportHelpers.FieldHandlers.Value[Type];
            }
        }

        public int? GetColumnByType(DeviceImportFieldTypes FieldType)
        {
            if (columnsByType == null)
                throw new ArgumentNullException(nameof(columnsByType));

            if (columnsByType.TryGetValue(FieldType, out var column))
            {
                return column.Index;
            }
            else
            {
                return null;
            }
        }

        protected void SetDatasetName(string DatasetName)
        {
            this.DatasetName = DatasetName;
        }

        protected void SetColumns(IEnumerable<DeviceImportColumn> Columns)
        {
            columns = Columns.ToList();
            columnsByType = columns
                .Where(c => c.Type != DeviceImportFieldTypes.IgnoreColumn)
                .ToDictionary(c => c.Type);
        }

        public void GuessHeaderTypes()
        {
            var dataReader = GetDataReader();
            foreach (var fieldHandler in DeviceImportHelpers.FieldHandlers.Value)
            {
                dataReader.Reset();

                var instance = (DeviceImportFieldBase)Activator.CreateInstance(fieldHandler.Value);
                var column = instance.GuessColumn(this, dataReader);

                if (column.HasValue)
                    SetColumnType(column.Value, instance.FieldType);
            }
        }

        public void UpdateColumnTypes(List<DeviceImportFieldTypes> columnTypes)
        {
            if (columnTypes == null)
                throw new ArgumentNullException(nameof(columnTypes));

            if (columnTypes.Count != columns.Count)
                throw new ArgumentException("The number of Column Types supplied does not match the number of Headers", nameof(columnTypes));

            if (!columnTypes.Any(h => h == DeviceImportFieldTypes.DeviceSerialNumber))
                throw new ArgumentException("At least one column must be the Device Serial Number", nameof(columnTypes));

            if (columnTypes.Where(h => h != DeviceImportFieldTypes.IgnoreColumn).GroupBy(h => h, (k, i) => Tuple.Create(k, i.Count())).Any(g => g.Item2 > 1))
                throw new ArgumentException("Column types can only be specified once for each type", nameof(columnTypes));

            for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                SetColumnType(columnIndex, columnTypes[columnIndex]);
            }
        }

        public virtual void Parse(IScheduledTaskStatus status)
        {
            if (ColumnCount == 0)
                throw new InvalidOperationException("No columns were found");

            if (!GetColumnByType(DeviceImportFieldTypes.DeviceSerialNumber).HasValue)
                throw new InvalidOperationException("At least one column must be the Device Serial Number");

            if (RecordCount == 0)
                throw new InvalidOperationException("No data was found in the import file");

            using (var database = DiscoDataContextProvider.Create())
            {
                var state = new DeviceImportDatabaseParsingState(database);

                var deviceSerialNumberIndex = GetColumnByType(DeviceImportFieldTypes.DeviceSerialNumber).Value;
                var columns = Columns
                    .Where(h => h.Type != DeviceImportFieldTypes.IgnoreColumn)
                    .ToList();

                status.UpdateStatus(0, "Parsing Import Records", "Starting...");

                var records = new List<(DeviceImportRecord Record, Device ExistingDevice)>();

                // Read all records and parse fields
                var dataReader = GetDataReader();
                while (dataReader.Read())
                {
                    string deviceSerialNumber = DeviceSerialNumberImportField.ParseRawDeviceSerialNumber(dataReader.GetValue(deviceSerialNumberIndex).GetString());

                    status.UpdateStatus(dataReader.Index, RecordCount, $"Parsing: {deviceSerialNumber}");

                    Device existingDevice = null;
                    if (DeviceSerialNumberImportField.IsDeviceSerialNumberValid(deviceSerialNumber))
                        existingDevice = state.FindDevice(deviceSerialNumber);

                    var fields = columns.Select(h =>
                    {
                        var f = (DeviceImportFieldBase)h.GetHandlerInstance();
                        var valueReader = dataReader.GetValue(h.Index);
                        f.Parse(state, existingDevice, valueReader);
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

                    var record = new DeviceImportRecord(dataReader.Index, deviceSerialNumber, fields, recordAction);
                    records.Add((record, existingDevice));
                }

                // Evaluate parsed records for any additional validation that requires knowledge of other records
                foreach (var (record, existingDevice) in records)
                    foreach (var field in record.Fields.Where(f => f.FieldAction.HasValue).Cast<DeviceImportFieldBase>())
                        field.Parsed(state, existingDevice, records.Select(r => r.Record), record);

                Records = records
                    .Select(r => r.Record)
                    .Cast<IDeviceImportRecord>().ToList();
            }
        }

        public void Apply(IScheduledTaskStatus status)
        {
            if (Records == null)
                throw new InvalidOperationException("Import Records have not been parsed");

            if (Records.Count == 0)
                throw new InvalidOperationException("There are no records to import");

            status.UpdateStatus(0, "Applying Import Records to Database", "Starting...");

            int affectedRecords = 0;

            using (var database = DiscoDataContextProvider.Create())
            {
                foreach (var (record, index) in Records.Select((r, i) => Tuple.Create(r, i)))
                {
                    status.UpdateStatus(index, Records.Count, $"Applying: {record.DeviceSerialNumber}");

                    if (DeviceImportRecord.Apply(record, database, newAllowsUnauthenticatedEnrol: true, defaultDeviceProfileId: null))
                        affectedRecords++;
                }
            }

            AffectedRecords = affectedRecords;
        }
    }
}
