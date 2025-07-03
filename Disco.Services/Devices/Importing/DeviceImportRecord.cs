﻿using Disco.Data.Repository;
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
        public int Index { get; private set; }
        public string DeviceSerialNumber { get; private set; }

        public IEnumerable<IDeviceImportField> Fields { get; private set; }

        public EntityState RecordAction { get; private set; }

        public bool HasError
        {
            get { return Fields.Any(f => !f.FieldAction.HasValue); }
        }

        internal DeviceImportRecord(int Index, string DeviceSerialNumber, IEnumerable<IDeviceImportField> Fields, EntityState RecordAction)
        {
            this.Index = Index;
            this.DeviceSerialNumber = DeviceSerialNumber;
            this.Fields = Fields;
            this.RecordAction = RecordAction;
        }

        public static bool Apply(IDeviceImportRecord record, DiscoDataContext Database)
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
                    device = Database.Devices.Find(record.DeviceSerialNumber);
                }
                else if (record.RecordAction == EntityState.Added)
                {
                    // Use 'Add Device Offline' default if available
                    var deviceProfileId = Database.DiscoConfiguration.DeviceProfiles.DefaultAddDeviceOfflineDeviceProfileId;
                    if (deviceProfileId == 0)
                    {
                        deviceProfileId = Database.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId;
                    }

                    // Create Device
                    device = new Device()
                    {
                        SerialNumber = record.DeviceSerialNumber.ToUpper(),
                        CreatedDate = DateTime.Now,
                        AllowUnauthenticatedEnrol = true,
                        DeviceProfileId = deviceProfileId,
                        DeviceModelId = 1 // Default 'Unknown Device Model'
                    };
                    Database.Devices.Add(device);
                }
                else
                {
                    // Invalid State
                    return false;
                }

                bool changesMade = (record.RecordAction == EntityState.Added);

                foreach (var field in record.Fields.Cast<DeviceImportFieldBase>())
                {
                    changesMade = field.Apply(Database, device) || changesMade;
                }

                // Commit Changes
                if (changesMade)
                    Database.SaveChanges();

                bool adDescriptionSet = false;

                foreach (var field in record.Fields.Cast<DeviceImportFieldBase>())
                {
                    field.Applied(Database, device, ref adDescriptionSet);
                }

                return changesMade;
            }

            // Record has Errors
            return false;
        }
    }
}
