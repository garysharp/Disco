using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing.Fields
{
    internal class DeviceDecommissionedReasonImportField : DeviceImportFieldBase
    {
        private string rawValue;
        private DecommissionReasons? parsedValue;
        private DecommissionReasons? previousValue;
        private bool setDate { get; set; }
        private static Lazy<Dictionary<string, DecommissionReasons>> decommissionReasonsMap = new Lazy<Dictionary<string, DecommissionReasons>>(() => BuildDecommissionReasonsMap());

        public override DeviceImportFieldTypes FieldType { get { return DeviceImportFieldTypes.DeviceDecommissionedReason; } }

        public override object RawParsedValue { get { return parsedValue; } }
        public override string FriendlyValue { get { return parsedValue.HasValue ? parsedValue.Value.ToString() : rawValue; } }
        public override string FriendlyPreviousValue { get { return previousValue.HasValue ? previousValue.Value.ToString() : null; } }

        public static DeviceDecommissionedReasonImportField Create(Device device, DecommissionReasons? decommissionReason, bool setDate, bool isUnassigningUser)
        {
            var field = new DeviceDecommissionedReasonImportField()
            {
                rawValue = decommissionReason?.ToString(),
                parsedValue = decommissionReason,
                previousValue = device.DecommissionReason,
                setDate = setDate
            };
            var hasOpenJobs = device.Jobs.Any(j => !j.ClosedDate.HasValue);
            if (!DeviceDecommissionedDateImportField.CanDecommissionDevice(device, isUnassigningUser, null, hasOpenJobs, out var errorMessage))
                field.Error(errorMessage);
            else if (device.DecommissionReason == decommissionReason)
                field.Success(EntityState.Unchanged);
            else
                field.Success(EntityState.Modified);

            return field;
        }

        public override bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, IDeviceImportContext Context, string DeviceSerialNumber, Device ExistingDevice, List<IDeviceImportRecord> PreviousRecords, IDeviceImportDataReader DataReader, int ColumnIndex)
        {
            var value = DataReader.GetString(ColumnIndex);

            if (string.IsNullOrWhiteSpace(value))
            {
                rawValue = null;
                parsedValue = null;
            }
            else
            {
                if (!decommissionReasonsMap.Value.TryGetValue(value.Trim(), out var valueReason))
                {
                    rawValue = value.Trim();
                    return Error("Cannot parse the value as a Decommission Reason");
                }
                else
                {
                    parsedValue = valueReason;
                }
            }

            var decommissionedDateIndex = Context.GetColumnByType(DeviceImportFieldTypes.DeviceDecommissionedDate);
            if (parsedValue.HasValue && !decommissionedDateIndex.HasValue)
            {
                if (!DeviceDecommissionedDateImportField.CanDecommissionDevice(ExistingDevice, Context, DataReader, out var errorMessage))
                    return Error(errorMessage);

                setDate = true;
            }
            else if (parsedValue.HasValue && string.IsNullOrWhiteSpace(DataReader.GetString(decommissionedDateIndex.Value)))
            {
                setDate = true;
            }

            if (ExistingDevice != null && ExistingDevice.DecommissionReason != parsedValue)
            {
                previousValue = ExistingDevice.DecommissionReason;
                return Success(EntityState.Modified);
            }
            else
                return Success(EntityState.Unchanged);
        }

        public override bool Apply(DiscoDataContext Database, Device Device)
        {
            if (FieldAction == EntityState.Modified)
            {
                // Decommission or Recommission Device
                Device.DecommissionReason = parsedValue;

                if (setDate)
                {
                    if (parsedValue.HasValue && !Device.DecommissionedDate.HasValue)
                    {
                        Device.DecommissionedDate = DateTime.Now;
                    }
                    else if (!parsedValue.HasValue && Device.DecommissionedDate.HasValue)
                    {
                        Device.DecommissionedDate = null;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Applied(DiscoDataContext Database, Device Device, ref bool DeviceADDescriptionSet)
        {
            // Only Enable/Disable if DeviceDecommissionedDate field is not present
            if (setDate)
            {
                if (ActiveDirectory.IsValidDomainAccountId(Device.DeviceDomainId))
                {
                    var adAccount = Device.ActiveDirectoryAccount();

                    if (adAccount != null && !adAccount.IsCriticalSystemObject)
                    {
                        if (Device.DecommissionedDate.HasValue)
                        {
                            // Disable AD Account
                            adAccount.DisableAccount();
                        }
                        else
                        {
                            // Enable AD Account
                            adAccount.EnableAccount();
                        }

                        if (!DeviceADDescriptionSet)
                        {
                            try
                            {
                                adAccount.SetDescription(Device);
                            }
                            catch (Exception ex)
                            {
                                SystemLog.LogWarning($"Unable to update AD Machine Account Description for {Device.DeviceDomainId}: {ex.Message}");
                                throw;
                            }
                            DeviceADDescriptionSet = true;
                        }
                    }
                }
            }
        }

        public override int? GuessColumn(DiscoDataContext Database, IDeviceImportContext Context, IDeviceImportDataReader DataReader)
        {
            // column name
            var possibleColumns = Context.Columns
                .Where(h => h.Type == DeviceImportFieldTypes.IgnoreColumn &&
                    h.Name.IndexOf("decommission reason", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("decommissionreason", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("decommissioned reason", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("decommissionedreason", StringComparison.OrdinalIgnoreCase) >= 0
                    );

            return possibleColumns.Select(h => (int?)h.Index).FirstOrDefault();
        }

        public static Dictionary<string, DecommissionReasons> BuildDecommissionReasonsMap()
        {
            return Enum.GetValues(typeof(DecommissionReasons)).Cast<DecommissionReasons>().SelectMany(r =>
            {
                var rCamelName = r.ToString();
                var rName = string.Join(string.Empty, rCamelName.SelectMany(c => char.IsUpper(c) ? new char[] { ' ', c } : new char[] { c })).Trim();
                if (rCamelName == rName)
                    return new Tuple<DecommissionReasons, string>[] { Tuple.Create(r, rCamelName) };
                else
                    return new Tuple<DecommissionReasons, string>[] { Tuple.Create(r, rCamelName), Tuple.Create(r, rName) };
            }).ToDictionary(k => k.Item2, v => v.Item1, StringComparer.OrdinalIgnoreCase);
        }
    }
}