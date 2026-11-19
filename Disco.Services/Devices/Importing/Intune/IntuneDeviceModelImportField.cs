using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using System;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing.Intune
{
    internal class IntuneDeviceModelImportField : DeviceImportFieldBase
    {
        internal DeviceModel DeviceModel { get; private set; }
        private DeviceModel PreviousDeviceModel;
        internal string ManufacturerName { get; private set; }
        internal string ModelName { get; private set; }

        public override DeviceImportFieldTypes FieldType => DeviceImportFieldTypes.ModelId;

        public override object RawParsedValue => throw new NotSupportedException();
        public override string FriendlyValue => DeviceModel?.ToString() ?? $"{ManufacturerName} {ModelName}";
        public override string FriendlyPreviousValue => PreviousDeviceModel?.ToString() ?? null;

        public override bool Parse(DeviceImportDatabaseParsingState state, Device existingDevice, IDeviceImportValueReader valueReader)
        {
            var intuneValueReader = valueReader as IntuneDeviceImport.IntuneDeviceImportValueReader
                ?? throw new InvalidOperationException("ValueReader must be of type IntuneDeviceImportValueReader");

            ManufacturerName = intuneValueReader.Record.Manufacturer?.Trim();
            ModelName = intuneValueReader.Record.Model?.Trim();

            PreviousDeviceModel = existingDevice?.DeviceModel;

            if (string.IsNullOrWhiteSpace(ManufacturerName) && string.IsNullOrWhiteSpace(ModelName))
                return Success(EntityState.Unchanged);

            if (string.IsNullOrWhiteSpace(ManufacturerName))
                ManufacturerName = "Unknown";
            if (string.IsNullOrWhiteSpace(ModelName))
                ModelName = "Unknown";

            DeviceModel = state.DeviceModels.FirstOrDefault(m =>
                m.Manufacturer.Equals(ManufacturerName, StringComparison.OrdinalIgnoreCase) &&
                m.Model.Equals(ModelName, StringComparison.OrdinalIgnoreCase));

            if (PreviousDeviceModel != null)
            {
                // only update if the value is newer
                if (DeviceModel != null && DeviceModel.Id == PreviousDeviceModel.Id)
                    return Success(EntityState.Unchanged);

                return Success(EntityState.Modified);
            }

            return Success(EntityState.Added);
        }

        internal void SetDeviceModel(DeviceModel deviceModel)
        {
            if (deviceModel == null)
                throw new ArgumentNullException(nameof(deviceModel));
            if (!string.Equals(deviceModel.Manufacturer, ManufacturerName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("DeviceModel does not match the ManufacturerName", nameof(deviceModel));
            if (!string.Equals(deviceModel.Model, ModelName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("DeviceModel does not match the ModelName", nameof(deviceModel));

            DeviceModel = deviceModel;
        }

        public override bool Apply(DiscoDataContext database, Device device)
        {
            if (FieldAction == EntityState.Added ||
                FieldAction == EntityState.Modified)
            {
                if (DeviceModel == null)
                    throw new InvalidOperationException("DeviceModel must be set before applying");

                device.DeviceModelId = DeviceModel.Id;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int? GuessColumn(IDeviceImportContext context, IDeviceImportDataReader dataReader)
            => throw new NotImplementedException();
    }
}
