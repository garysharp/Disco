using System;
using System.Collections.Generic;

namespace Disco.Models.Services.Devices.Importing
{
    public interface IDeviceImportDataReader : IDisposable
    {
        void Reset();
        bool Read();

        int Index { get; }

        int GetRowNumber(int Index);

        string GetString(int ColumnIndex);
        IEnumerable<string> GetStrings(int ColumnIndex);

        bool TryGetNullableInt(int ColumnIndex, out int? value);

        bool TryGetNullableBool(int ColumnIndex, out bool? value);

        bool TryGetNullableDateTime(int ColumnIndex, out DateTime? value);

        bool TestAllNotEmpty(int ColumnIndex);
        bool TestAllNullableInt(int ColumnIndex);
        bool TestAllInt(int ColumnIndex);
        bool TestAllNullableBool(int ColumnIndex);
        bool TestAllNullableDateTime(int ColumnIndex);
    }
}
