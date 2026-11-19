using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace Disco.Services.Devices.Importing.Google
{
    internal class GoogleDeviceModelImportField : DeviceImportFieldBase
    {
        private DeviceModel previousModel;
        internal DeviceModel DeviceModel { get; private set; }
        internal string ModelName { get; private set; }

        public override DeviceImportFieldTypes FieldType => DeviceImportFieldTypes.ModelId;
        public override object RawParsedValue => DeviceModel?.Id;
        public override string FriendlyValue => DeviceModel?.ToString() ?? ModelName;
        public override string FriendlyPreviousValue => previousModel?.ToString();

        public override bool Parse(DeviceImportDatabaseParsingState state, Device existingDevice, IDeviceImportValueReader valueReader)
        {
            var modelName = valueReader.GetString()?.Trim();
            ModelName = modelName;

            if (string.IsNullOrWhiteSpace(modelName))
                return Success(EntityState.Unchanged);

            var firstModelName = Regex.Match(modelName, @"^\s*(?<model>(?:[^()/,()]|/(?(depth)|(?!))|,(?(depth)|(?!))|\((?<depth>)|\)(?<-depth>))+?)(?(depth)(?!))\s*(?:[/,]|$)", RegexOptions.None, TimeSpan.FromMilliseconds(100));

            if (!firstModelName.Success)
                return Error("Invalid model name format");

            modelName = firstModelName.Groups["model"].Value.Trim();
            ModelName = modelName;

            if (modelName.Length > 200)
                return Error("Cannot be more than 200 characters");

            previousModel = existingDevice == null ? null : state.DeviceModels.FirstOrDefault(m => m.Id == existingDevice.DeviceModelId);
            if (previousModel != null && string.Equals($"{previousModel.Manufacturer} {previousModel.Model}", modelName, StringComparison.OrdinalIgnoreCase))
            {
                DeviceModel = previousModel;
                return Success(EntityState.Unchanged);
            }

            // Google exports a model name but no separate manufacturer.
            DeviceModel = state.DeviceModels.FirstOrDefault(m => string.Equals($"{m.Manufacturer} {m.Model}", modelName, StringComparison.OrdinalIgnoreCase));

            return Success(existingDevice == null ? EntityState.Added : EntityState.Modified);
        }

        internal void SetDeviceModel(DeviceModel deviceModel) => DeviceModel = deviceModel;

        public override bool Apply(DiscoDataContext database, Device device)
        {
            if (FieldAction != EntityState.Added && FieldAction != EntityState.Modified)
                return false;

            device.DeviceModelId = DeviceModel?.Id ?? throw new InvalidOperationException("Device model must be resolved before applying");
            return true;
        }

        public override int? GuessColumn(IDeviceImportContext context, IDeviceImportDataReader dataReader)
            => throw new NotImplementedException();
    }
}
