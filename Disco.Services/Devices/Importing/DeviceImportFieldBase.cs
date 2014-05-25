using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public abstract bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, DeviceImportContext Context, int RecordIndex, string DeviceSerialNumber, Device ExistingDevice, Dictionary<DeviceImportFieldTypes, string> Values, string Value);
        public abstract bool Apply(DiscoDataContext Database, Device Device);

        public abstract int? GuessHeader(DiscoDataContext Database, DeviceImportContext Context);

        #region Helpers
        protected bool Error(string Message)
        {
            this.ErrorMessage = Message;
            this.FieldAction = null;
            return false;
        }
        protected bool Success(EntityState Action)
        {
            this.FieldAction = Action;
            return true;
        }
        #endregion
    }
}
