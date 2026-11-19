using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using System.Collections.Generic;
using System.Data;

namespace Disco.Services.Devices.Importing
{
    internal abstract class DeviceImportFieldBase : IDeviceImportField
    {
        public abstract DeviceImportFieldTypes FieldType { get; }

        public EntityState? FieldAction { get; protected set; }

        public string ErrorMessage { get; protected set; }

        public abstract object RawParsedValue { get; }
        public abstract string FriendlyValue { get; }
        public abstract string FriendlyPreviousValue { get; }

        public abstract bool Parse(DeviceImportDatabaseParsingState state, Device existingDevice, IDeviceImportValueReader valueReader);
        public virtual bool Parsed(DeviceImportDatabaseParsingState state, Device existingDevice, IEnumerable<IDeviceImportRecord> records, IDeviceImportRecord record) => true;
        public abstract bool Apply(DiscoDataContext database, Device device);
        public virtual void Applied(DiscoDataContext database, Device device, ref bool deviceADDescriptionSet) { }

        public abstract int? GuessColumn(IDeviceImportContext context, IDeviceImportDataReader dataReader);

        #region Helpers
        protected bool Error(string message)
        {
            ErrorMessage = message;
            FieldAction = null;
            return false;
        }
        protected bool Success(EntityState action)
        {
            FieldAction = action;
            return true;
        }
        #endregion
    }
}
