using System.Data;

namespace Disco.Models.Services.Devices.Importing
{
    public interface IDeviceImportField
    {
        DeviceImportFieldTypes FieldType { get; }
        EntityState? FieldAction { get; }

        string ErrorMessage { get; }

        object RawParsedValue { get; }
        string FriendlyValue { get; }
        string FriendlyPreviousValue { get; }
    }
}
