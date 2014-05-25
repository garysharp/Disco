using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

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

        public override bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, DeviceImportContext Context, int RecordIndex, string DeviceSerialNumber, Device ExistingDevice, Dictionary<DeviceImportFieldTypes, string> Values, string Value)
        {
            if (string.IsNullOrWhiteSpace(Value))
            {
                rawValue = null;
                parsedValue = null;
            }
            else
            {
                DecommissionReasons valueReason;
                if (!decommissionReasonsMap.Value.TryGetValue(Value.Trim(), out valueReason))
                {
                    rawValue = Value.Trim();
                    return Error("Cannot parse the value as a Decommission Reason");
                }
                else
                {
                    parsedValue = valueReason;
                }
            }

            if (parsedValue.HasValue && !Values.ContainsKey(DeviceImportFieldTypes.DeviceDecommissionedDate))
            {
                string errorMessage;
                if (!DeviceDecommissionedDateImportField.CanDecommissionDevice(ExistingDevice, Values, out errorMessage))
                    return Error(errorMessage);

                setDate = true;
            }
            else if (parsedValue.HasValue && string.IsNullOrWhiteSpace(Values[DeviceImportFieldTypes.DeviceDecommissionedDate]))
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
            if (this.FieldAction == EntityState.Modified)
            {
                // Decommission or Recommission Device
                Device.DecommissionReason = this.parsedValue;

                if (setDate)
                    Device.DecommissionedDate = this.parsedValue.HasValue ? (DateTime?)DateTime.Now : null;

                return true;
            }
            else
            {
                return false;
            }
        }

        public override int? GuessHeader(DiscoDataContext Database, DeviceImportContext Context)
        {
            // column name
            var possibleColumns = Context.Header
                .Select((h, i) => Tuple.Create(h, i))
                .Where(h => h.Item1.Item2 == DeviceImportFieldTypes.IgnoreColumn &&
                    h.Item1.Item1.IndexOf("decommission reason", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Item1.Item1.IndexOf("decommissionreason", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Item1.Item1.IndexOf("decommissioned reason", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Item1.Item1.IndexOf("decommissionedreason", System.StringComparison.OrdinalIgnoreCase) >= 0
                    );

            return possibleColumns.Select(h => (int?)h.Item2).FirstOrDefault();
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