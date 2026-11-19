using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Devices.Importing.Fields;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace Disco.Services.Devices.Importing.Decommissioning
{
    public class DeviceDecommissionImportContext : DeviceImportContextBase
    {
        private enum Scope { Batch, Profile, Model }
        private readonly Scope scope;
        private readonly int scopeId;
        private readonly DecommissionReasons decommissionReason;
        private readonly bool unassignUsers;

        public override int RecordCount => Records.Count;

        private DeviceDecommissionImportContext(string sourceName, Scope scope, int scopeId, DecommissionReasons decommissionReason, bool unassignUsers)
            : base(sourceName)
        {
            this.scope = scope;
            this.scopeId = scopeId;
            this.decommissionReason = decommissionReason;
            this.unassignUsers = unassignUsers;
            AllowBacktracking = false;
            SetDatasetName(sourceName);
        }

        public static DeviceDecommissionImportContext Create(DiscoDataContext database, DeviceBatch deviceBatch, DecommissionReasons decommissionReason, bool unassignUsers)
            => new DeviceDecommissionImportContext($"Batch: {deviceBatch.Name} ({deviceBatch.Id})", Scope.Batch, deviceBatch.Id, decommissionReason, unassignUsers);

        public static DeviceDecommissionImportContext Create(DiscoDataContext database, DeviceProfile deviceProfile, DecommissionReasons decommissionReason, bool unassignUsers)
            => new DeviceDecommissionImportContext($"Profile: {deviceProfile.Name} ({deviceProfile.Id})", Scope.Profile, deviceProfile.Id, decommissionReason, unassignUsers);

        public static DeviceDecommissionImportContext Create(DiscoDataContext database, DeviceModel deviceModel, DecommissionReasons decommissionReason, bool unassignUsers)
            => new DeviceDecommissionImportContext($"Model: {deviceModel.Description} ({deviceModel.Id})", Scope.Model, deviceModel.Id, decommissionReason, unassignUsers);

        public override void Parse(IScheduledTaskStatus status)
        {
            var importColumns = new List<DeviceImportColumn>(3)
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

            using (var database = DiscoDataContextProvider.Create())
            {

                var devices = CalculateDevices(database);

                var doUnassignUsers = unassignUsers;

                if (doUnassignUsers && devices.Any(d => d.AssignedUserId != null))
                {
                    importColumns.Add(new DeviceImportColumn()
                    {
                        Index = 2,
                        Type = DeviceImportFieldTypes.AssignedUserId,
                        Handler = typeof(AssignedUserIdImportField),
                        Name = "Assigned User Identifier",
                    });
                }
                else
                {
                    doUnassignUsers = false;
                }

                columns = importColumns;

                Records = devices.Select<Device, IDeviceImportRecord>((d, i) =>
                {
                    var fields = new List<IDeviceImportField>(ColumnCount)
                    {
                    DeviceSerialNumberImportField.Create(d),
                    DeviceDecommissionedReasonImportField.Create(d, decommissionReason, true, doUnassignUsers),
                    };
                    if (doUnassignUsers)
                    {
                        fields.Add(AssignedUserIdImportField.CreateUnassigned(d));
                    }
                    return new DeviceDecommissionImportRecord(d, i, fields);
                }).ToList();
            }
        }

        private IEnumerable<Device> CalculateDevices(DiscoDataContext database)
        {
            var query = database.Devices
                .Include(d => d.Jobs)
                .Include(d => d.AssignedUser);

            if (scope == Scope.Batch)
                query = query.Where(d => d.DeviceBatchId == scopeId && d.DecommissionedDate == null);
            else if (scope == Scope.Model)
                query = query.Where(d => d.DeviceModelId == scopeId && d.DecommissionedDate == null);
            else if (scope == Scope.Profile)
                query = query.Where(d => d.DeviceProfileId == scopeId && d.DecommissionedDate == null);
            else
                throw new InvalidOperationException($"Invalid scope: {scope}");

            return query.ToList();
        }

        public override IDeviceImportDataReader GetDataReader()
            => throw new NotImplementedException();
    }
}
