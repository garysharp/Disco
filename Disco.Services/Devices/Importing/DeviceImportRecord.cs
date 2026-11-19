using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing
{
    internal class DeviceImportRecord : IDeviceImportRecord
    {
        public int Index { get; }
        public string DeviceSerialNumber { get; }

        public IEnumerable<IDeviceImportField> Fields { get; }

        public EntityState RecordAction { get; }

        public bool HasError
            => Fields.Any(f => !f.FieldAction.HasValue);

        internal DeviceImportRecord(int index, string deviceSerialNumber, IEnumerable<DeviceImportFieldBase> fields, EntityState recordAction)
        {
            Index = index;
            DeviceSerialNumber = deviceSerialNumber;
            Fields = fields.Cast<IDeviceImportField>();
            RecordAction = recordAction;
        }

        public static bool Apply(IDeviceImportRecord record, DiscoDataContext database, bool newAllowsUnauthenticatedEnrol, int? defaultDeviceProfileId)
        {
            if (record.RecordAction == EntityState.Detached || !record.HasError)
            {
                Device device;

                if (record.RecordAction == EntityState.Unchanged)
                {
                    // Unchanged - No Action Required
                    return false;
                }
                else if (record.RecordAction == EntityState.Modified)
                {
                    device = database.Devices.Find(record.DeviceSerialNumber);
                }
                else if (record.RecordAction == EntityState.Added)
                {
                    // Use 'Add Device Offline' default if available
                    var deviceProfileId = defaultDeviceProfileId ?? database.DiscoConfiguration.DeviceProfiles.DefaultAddDeviceOfflineDeviceProfileId;
                    if (deviceProfileId == 0)
                    {
                        deviceProfileId = database.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId;
                    }

                    // Create Device
                    device = new Device()
                    {
                        SerialNumber = record.DeviceSerialNumber.ToUpper(),
                        CreatedDate = DateTime.Now,
                        AllowUnauthenticatedEnrol = newAllowsUnauthenticatedEnrol,
                        DeviceProfileId = deviceProfileId,
                        DeviceModelId = 1 // Default 'Unknown Device Model'
                    };
                    database.Devices.Add(device);
                }
                else
                {
                    // Invalid State
                    return false;
                }

                bool changesMade = record.RecordAction == EntityState.Added;

                foreach (var field in record.Fields.Cast<DeviceImportFieldBase>())
                {
                    changesMade = field.Apply(database, device) || changesMade;
                }

                // Commit Changes
                if (changesMade)
                    database.SaveChanges();

                bool adDescriptionSet = false;

                foreach (var field in record.Fields.Cast<DeviceImportFieldBase>())
                {
                    field.Applied(database, device, ref adDescriptionSet);
                }

                return changesMade;
            }

            // Record has Errors
            return false;
        }
    }
}
