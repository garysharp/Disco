using Disco.BI.Extensions;
using Disco.Data.Repository;
using Disco.Models.BI.Device;
using Disco.Models.Repository;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using PopulateRecordReferences = System.Tuple<System.Collections.Generic.Dictionary<int, Disco.Models.Repository.DeviceModel>, System.Collections.Generic.Dictionary<int, Disco.Models.Repository.DeviceProfile>, System.Collections.Generic.Dictionary<int, Disco.Models.Repository.DeviceBatch>>;

namespace Disco.BI.DeviceBI.Importing
{
    public static class Import
    {
        internal const string ImportParseCacheKey = "ImportParseResults_{0}";

        public static ImportDeviceSession GetSession(string ImportParseTaskId)
        {
            string parseKey = string.Format(ImportParseCacheKey, ImportParseTaskId);

            return (ImportDeviceSession)HttpRuntime.Cache.Get(parseKey);
        }

        internal static bool ImportRecord(this ImportDevice device, DiscoDataContext Database, PopulateRecordReferences references)
        {
            // Skips If Errors
            if (device.Errors == null || device.Errors.Count == 0)
            {
                // Re-Populate & Skip If Errors
                device.PopulateRecord(Database, references);
                if (device.Errors == null || device.Errors.Count == 0)
                {
                    Device discoDevice = device.Device;

                    if (discoDevice == null)
                    {
                        // New Device
                        discoDevice = new Device()
                        {
                            SerialNumber = device.SerialNumber.ToUpper(),
                            CreatedDate = DateTime.Now,
                            AllowUnauthenticatedEnrol = true,
                        };
                        Database.Devices.Add(discoDevice);
                    }

                    if (discoDevice.DeviceModelId != device.DeviceModelId)
                        discoDevice.DeviceModelId = device.DeviceModelId;
                    if (discoDevice.DeviceProfileId != device.DeviceProfileId)
                        discoDevice.DeviceProfileId = device.DeviceProfileId;
                    if (discoDevice.DeviceBatchId != device.DeviceBatchId)
                        discoDevice.DeviceBatchId = device.DeviceBatchId;
                    if (discoDevice.Location != device.Location)
                        discoDevice.Location = device.Location;
                    if (discoDevice.AssetNumber != device.AssetNumber)
                        discoDevice.AssetNumber = device.AssetNumber;

                    if (discoDevice.AssignedUserId != device.AssignedUserId)
                    {
                        discoDevice.AssignDevice(Database, device.AssignedUser);
                    }

                    Database.SaveChanges();

                    return true;
                }
            }
            return false;
        }

        internal static PopulateRecordReferences GetPopulateRecordReferences(DiscoDataContext Database)
        {
            return new PopulateRecordReferences(
                Database.DeviceModels.ToDictionary(dm => dm.Id),
                Database.DeviceProfiles.ToDictionary(dp => dp.Id),
                Database.DeviceBatches.ToDictionary(db => db.Id)
                );
        }

        internal static void PopulateRecord(this ImportDevice device, DiscoDataContext Database, PopulateRecordReferences references)
        {

            var deviceModels = references.Item1;
            var deviceProfiles = references.Item2;
            var deviceBatches = references.Item3;

            // SERIAL NUMBER - Existing Device
            if (!device.Errors.ContainsKey("SerialNumber"))
            {
                device.Device = Database.Devices.Find(device.SerialNumber);
                if (device.Device != null && device.Device.DecommissionedDate.HasValue)
                    device.Errors.Add("SerialNumber", "The device is decommissioned");
            }


            // DEVICE MODEL
            if (!device.Errors.ContainsKey("DeviceModelId"))
            {
                DeviceModel deviceModel;

                if (!device.DeviceModelId.HasValue)
                    device.DeviceModelId = 1; // Default 'Unknown Device Model'

                if (!deviceModels.TryGetValue(device.DeviceModelId.Value, out deviceModel))
                    device.Errors.Add("DeviceModelId", string.Format("Unknown device model id: {0}", device.DeviceModelId));
                else
                    device.DeviceModel = deviceModel;
            }

            // DEVICE PROFILE
            if (!device.Errors.ContainsKey("DeviceProfileId"))
            {
                DeviceProfile deviceProfile;
                if (!deviceProfiles.TryGetValue(device.DeviceProfileId, out deviceProfile))
                    device.Errors.Add("DeviceProfileId", string.Format("Unknown device profile id: {0}", device.DeviceProfileId));
                else
                    device.DeviceProfile = deviceProfile;
            }

            // DEVICE BATCH
            if (!device.Errors.ContainsKey("DeviceBatchId") && device.DeviceBatchId.HasValue)
            {
                DeviceBatch deviceBatch;
                if (!deviceBatches.TryGetValue(device.DeviceBatchId.Value, out deviceBatch))
                    device.Errors.Add("DeviceBatchId", string.Format("Unknown device Batch id: {0}", device.DeviceBatchId));
                else
                    device.DeviceBatch = deviceBatch;
            }

            // ASSIGNED USER
            if (!device.Errors.ContainsKey("AssignedUserId") && device.AssignedUserId != null)
            {
                try
                {
                    device.AssignedUser = UserService.GetUser(device.AssignedUserId, Database, true);
                }
                catch (ArgumentException)
                {
                    device.Errors.Add("AssignedUserId", string.Format("Unknown user id: {0}", device.AssignedUserId));
                }
            }

        }

        internal static ImportDevice ParseRecord(this string[] record)
        {
            int csvFieldCount = record.Length;
            if (csvFieldCount < 1)
                throw new ArgumentException("At least one CSV field is required (Serial Number)");

            string csvSerialNumber;
            string csvDeviceModelId;
            int deviceModelId = 1; // Default 'Unknown Device Model'
            string csvDeviceProfileId;
            int deviceProfileId = 1; // 'Default' Profile
            string csvDeviceBatchId;
            int deviceBatchId = 0;  // No Batch
            string csvAssignedUserId = null;
            string csvLocation = null;
            string csvAssetNumber = null;
            Dictionary<string, string> errors = new Dictionary<string, string>();

            // SERIAL NUMBER
            csvSerialNumber = record[0];
            if (string.IsNullOrWhiteSpace(csvSerialNumber))
                errors.Add("SerialNumber", "The serial number is required");
            else if (csvSerialNumber.Trim().Length > 60)
                errors.Add("SerialNumber", "The serial number must be less than or equal to 60 characters");

            if (csvFieldCount > 1)
            {
                // DEVICE MODEL
                csvDeviceModelId = record[1];
                if (!string.IsNullOrWhiteSpace(csvDeviceModelId))
                    if (!int.TryParse(csvDeviceModelId, out deviceModelId))
                        errors.Add("DeviceModelId", "The device model is optional, but when supplied must be a number");
                    else if (deviceModelId < 1)
                        errors.Add("DeviceModelId", "The device model is optional, but when supplied must be greater than 0");

                if (csvFieldCount > 2)
                {
                    // DEVICE PROFILE
                    csvDeviceProfileId = record[2];
                    if (!string.IsNullOrWhiteSpace(csvDeviceProfileId))
                        if (!int.TryParse(csvDeviceProfileId, out deviceProfileId))
                            errors.Add("DeviceProfileId", "The device profile is optional, but when supplied must be a number");
                        else if (deviceProfileId < 1)
                            errors.Add("DeviceProfileId", "The device profile is optional, but when supplied must be greater than 0");

                    if (csvFieldCount > 3)
                    {
                        // DEVICE BATCH
                        csvDeviceBatchId = record[3];
                        if (!string.IsNullOrWhiteSpace(csvDeviceBatchId))
                            if (!int.TryParse(csvDeviceBatchId, out deviceBatchId))
                                errors.Add("DeviceBatchId", "The device batch is optional, but when supplied must be a number");
                            else if (deviceBatchId < 1)
                                errors.Add("DeviceBatchId", "The device batch is optional, but when supplied must be greater than 0");

                        if (csvFieldCount > 4)
                        {
                            // ASSIGNED USER
                            csvAssignedUserId = record[4];
                            if (string.IsNullOrWhiteSpace(csvAssignedUserId))
                                csvAssignedUserId = null; // Not Assigned
                            else
                            {
                                if (csvAssignedUserId.Length > 50)
                                    errors.Add("AssignedUserId", "The assigned user must be less than or equal to 50 characters");
                                else if (!csvAssignedUserId.Contains('\\')) // Assume Primary Domain
                                    csvAssignedUserId = string.Format(@"{0}\{1}", ActiveDirectory.Context.PrimaryDomain.NetBiosName, csvAssignedUserId);
                            }

                            if (csvFieldCount > 5)
                            {
                                // LOCATION
                                csvLocation = record[5];
                                if (string.IsNullOrWhiteSpace(csvLocation))
                                    csvLocation = null; // No Location Specified
                                else if (csvLocation.Length > 250)
                                    errors.Add("Location", "The location must be less than or equal to 250 characters");

                                if (csvFieldCount > 6)
                                {
                                    // ASSET NUMBER
                                    csvAssetNumber = record[6];
                                    if (string.IsNullOrWhiteSpace(csvAssetNumber))
                                        csvAssetNumber = null; // No Location Specified
                                    else if (csvAssetNumber.Length > 40)
                                        errors.Add("AssetNumber", "The asset number must be less than or equal to 40 characters");
                                }
                            }
                        }
                    }
                }
            }

            return new ImportDevice()
            {
                SerialNumber = csvSerialNumber.Trim(),
                DeviceModelId = deviceModelId,
                DeviceProfileId = deviceProfileId,
                DeviceBatchId = deviceBatchId == 0 ? (int?)null : deviceBatchId,
                AssignedUserId = csvAssignedUserId,
                Location = csvLocation,
                AssetNumber = csvAssetNumber,
                Errors = errors
            };
        }

        #region ImportDevice Extensions

        public static string ImportStatus(this ImportDevice device)
        {
            if (device.Errors.Count > 0)
                return "Error";

            if (device.Device != null)
                return "Update";

            return "New";
        }

        #endregion

    }
}
