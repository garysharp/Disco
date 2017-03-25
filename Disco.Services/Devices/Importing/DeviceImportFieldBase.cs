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

        public abstract bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, IDeviceImportContext Context, string DeviceSerialNumber, Device ExistingDevice, List<IDeviceImportRecord> PreviousRecords, IDeviceImportDataReader DataReader, int ColumnIndex);
        public abstract bool Apply(DiscoDataContext Database, Device Device);
        public virtual void Applied(DiscoDataContext Database, Device Device, ref bool DeviceADDescriptionSet) { return; }

        public abstract int? GuessColumn(DiscoDataContext Database, IDeviceImportContext Context, IDeviceImportDataReader DataReader);

        #region Helpers
        protected bool Error(string Message)
        {
            ErrorMessage = Message;
            FieldAction = null;
            return false;
        }
        protected bool Success(EntityState Action)
        {
            FieldAction = Action;
            return true;
        }
        #endregion
    }
}
