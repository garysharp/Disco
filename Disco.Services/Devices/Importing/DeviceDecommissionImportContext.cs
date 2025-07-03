using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Devices.Importing.Fields;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace Disco.Services.Devices.Importing
{
    public class DeviceDecommissionImportContext : IDeviceImportContext
    {
        private readonly List<IDeviceImportRecord> records;

        public string SessionId { get; }
        public string Filename { get; }
        public string DatasetName { get; }
        public int ColumnCount { get; }
        public IEnumerable<IDeviceImportColumn> Columns { get; }

        public int RecordCount => records.Count;
        public List<IDeviceImportRecord> Records { get => records; set => throw new NotImplementedException(); }
        public int AffectedRecords { get; set; }
        public bool AllowBacktracking { get; } = false;

        private DeviceDecommissionImportContext(string sourceName, List<Device> devices, DecommissionReasons decommissionReason, bool unassignUsers)
        {
            SessionId = Guid.NewGuid().ToString("D");
            Filename = DatasetName = sourceName;

            var columns = new List<IDeviceImportColumn>(3)
            {
                new DeviceImportColumn()
                {
                    Index = 0,
                    Type = DeviceImportFieldTypes.DeviceSerialNumber,
                    Handler = typeof(DeviceSerialNumberImportField),
                    Name = "Device Serial Number",
                },
                new DeviceImportColumn()
                {
                    Index = 1,
                    Type = DeviceImportFieldTypes.DeviceDecommissionedReason,
                    Handler = typeof(DeviceDecommissionedReasonImportField),
                    Name = "Device Decommissioned Reason",
                }
            };

            if (unassignUsers && devices.Any(d => d.AssignedUserId != null))
            {
                ColumnCount = 3;
                columns.Add(new DeviceImportColumn()
                {
                    Index = 2,
                    Type = DeviceImportFieldTypes.AssignedUserId,
                    Handler = typeof(AssignedUserIdImportField),
                    Name = "Assigned User Identifier",
                });
            }
            else
            {
                unassignUsers = false;
            }

            Columns = columns;
            ColumnCount = columns.Count;

            records = devices.Select<Device, IDeviceImportRecord>((d, i) =>
            {
                var fields = new List<IDeviceImportField>(ColumnCount)
                {
                    DeviceSerialNumberImportField.Create(d),
                    DeviceDecommissionedReasonImportField.Create(d, decommissionReason, true, unassignUsers),
                };
                if (unassignUsers)
                {
                    fields.Add(AssignedUserIdImportField.CreateUnassigned(d));
                }
                return new DeviceDecommissionImportRecord(d, i, fields);
            }).ToList();

        }

        public static DeviceDecommissionImportContext Create(DiscoDataContext database, DeviceBatch deviceBatch, DecommissionReasons decommissionReason, bool unassignUsers)
        {
            var devices = database.Devices
                .Include(d => d.Jobs)
                .Include(d => d.AssignedUser)
                .Where(d => d.DeviceBatchId == deviceBatch.Id && d.DecommissionedDate == null)
                .ToList();
            return new DeviceDecommissionImportContext($"Batch: {deviceBatch.Name} ({deviceBatch.Id})", devices, decommissionReason, unassignUsers);
        }

        public IDeviceImportColumn GetColumn(int Index)
            => throw new NotImplementedException();

        public int? GetColumnByType(DeviceImportFieldTypes FieldType)
            => throw new NotImplementedException();

        public IDeviceImportDataReader GetDataReader()
            => throw new NotImplementedException();

        public IEnumerable<KeyValuePair<DeviceImportFieldTypes, Type>> GetFieldHandlers()
            => throw new NotImplementedException();

        public void SetColumnType(int Index, DeviceImportFieldTypes Type)
            => throw new NotImplementedException();
    }
}
