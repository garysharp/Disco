using System;

namespace Disco.Services.Devices.Importing
{
    public interface IDeviceImportValueReader
    {
        string GetString();

        bool TryGetNullableInt(out int? value);
        bool TryGetNullableBool(out bool? value);
        bool TryGetNullableDateTime(out DateTime? value);
    }
}
