using Disco.Models.Services.Devices.Importing;
using Disco.Services.Devices.Importing.Fields;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Devices.Importing
{
    public abstract class BaseDeviceImportContext : IDeviceImportContext
    {
        private static Lazy<Dictionary<DeviceImportFieldTypes, Type>> FieldHandlers = new Lazy<Dictionary<DeviceImportFieldTypes, Type>>(() =>
        {
            return new Dictionary<DeviceImportFieldTypes, Type>()
            {
                { DeviceImportFieldTypes.DeviceSerialNumber, typeof(DeviceSerialNumberImportField) },
                { DeviceImportFieldTypes.DeviceComputerName, typeof(DeviceComputerNameImportField) },
                { DeviceImportFieldTypes.DeviceAssetNumber, typeof(DeviceAssetNumberImportField) },
                { DeviceImportFieldTypes.DeviceLocation, typeof(DeviceLocationImportField) },
                { DeviceImportFieldTypes.DeviceAllowUnauthenticatedEnrol, typeof(DeviceAllowUnauthenticatedEnrolImportField) },
                { DeviceImportFieldTypes.DeviceDecommissionedDate, typeof(DeviceDecommissionedDateImportField) },
                { DeviceImportFieldTypes.DeviceDecommissionedReason, typeof(DeviceDecommissionedReasonImportField) },

                { DeviceImportFieldTypes.DetailLanMacAddress, typeof(DetailLanMacAddressImportField) },
                { DeviceImportFieldTypes.DetailWLanMacAddress, typeof(DetailWLanMacAddressImportField) },
                { DeviceImportFieldTypes.DetailACAdapter, typeof(DetailACAdapterImportField) },
                { DeviceImportFieldTypes.DetailBattery, typeof(DetailBatteryImportField) },
                { DeviceImportFieldTypes.DetailKeyboard, typeof(DetailKeyboardImportField) },

                { DeviceImportFieldTypes.ModelId, typeof(ModelIdImportField) },

                { DeviceImportFieldTypes.BatchId, typeof(BatchIdImportField) },

                { DeviceImportFieldTypes.ProfileId, typeof(ProfileIdImportField) },

                { DeviceImportFieldTypes.AssignedUserId, typeof(AssignedUserIdImportField) }
            };
        });

        private List<DeviceImportColumn> columns;
        private Dictionary<DeviceImportFieldTypes, DeviceImportColumn> columnsByType;

        public string SessionId { get; }
        public string Filename { get; }
        public string DatasetName { get; private set; }

        public abstract int RecordCount { get; }

        public List<IDeviceImportRecord> Records { get; set; }
        public int AffectedRecords { get; set; }

        public bool AllowBacktracking { get; } = true;

        public int ColumnCount { get { return columns.Count; } }
        public IEnumerable<IDeviceImportColumn> Columns
        {
            get
            {
                if (columns == null)
                    throw new ArgumentNullException(nameof(columns));

                return columns;
            }
        }

        public BaseDeviceImportContext(string Filename)
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

        public void SetColumnType(int Index, DeviceImportFieldTypes Type)
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
                column.Handler = FieldHandlers.Value[Type];
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

        public IEnumerable<KeyValuePair<DeviceImportFieldTypes, Type>> GetFieldHandlers()
        {
            return FieldHandlers.Value;
        }
    }
}
