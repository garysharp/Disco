using Disco.Models.Services.Devices.Importing;
using Disco.Services.Devices.Importing.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Disco.Services.Devices.Importing
{
    internal static class DeviceImportHelpers
    {
        public readonly static Lazy<Dictionary<DeviceImportFieldTypes, Type>> FieldHandlers = new Lazy<Dictionary<DeviceImportFieldTypes, Type>>(() =>
        {
            return new Dictionary<DeviceImportFieldTypes, Type>()
            {
                { DeviceImportFieldTypes.DeviceSerialNumber, typeof(DeviceSerialNumberImportField) },
                { DeviceImportFieldTypes.DeviceComputerName, typeof(DeviceComputerNameImportField) },
                { DeviceImportFieldTypes.DeviceDnsHostName, typeof(DeviceDnsHostNameImportField) },
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

        public static bool TryParseMacAddress(string value, out string macAddress)
        {
            macAddress = null;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim();
            if (!Regex.IsMatch(value, @"\A(?:[0-9a-fA-F]{12}|[0-9a-fA-F]{2}(?::[0-9a-fA-F]{2}){5}|[0-9a-fA-F]{2}(?:-[0-9a-fA-F]{2}){5}|[0-9a-fA-F]{4}(?:\.[0-9a-fA-F]{4}){2})\z", RegexOptions.None, TimeSpan.FromMilliseconds(100)))
                return false;

            var hex = value.Replace(":", "").Replace("-", "").Replace(".", "").ToUpperInvariant();
            macAddress = string.Join(":", Enumerable.Range(0, 6).Select(i => hex.Substring(i * 2, 2)));
            return true;
        }

        public static (bool IsDecommissioning, bool IsRecommissioning) GetDecommissioningState(this IDeviceImportRecord record)
        {
            bool decom = false;
            bool recom = false;
            var dateField = record.Fields.FirstOrDefault(f => f.FieldType == DeviceImportFieldTypes.DeviceDecommissionedDate);
            var readonField = record.Fields.FirstOrDefault(f => f.FieldType == DeviceImportFieldTypes.DeviceDecommissionedReason);
            if (dateField != null && readonField != null)
            {
                decom = dateField.RawParsedValue != null ||
                    readonField.RawParsedValue != null;
                recom = dateField.RawParsedValue == null &&
                    readonField.RawParsedValue == null;
            }
            else if (dateField != null)
            {
                decom = dateField.RawParsedValue != null;
                recom = dateField.RawParsedValue == null;

            }
            else if (readonField != null)
            {
                decom = readonField.RawParsedValue != null;
                recom = readonField.RawParsedValue == null;
            }
            return (decom, recom);
        }

    }
}
