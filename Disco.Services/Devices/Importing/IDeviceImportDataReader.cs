namespace Disco.Services.Devices.Importing
{
    public interface IDeviceImportDataReader
    {
        void Reset();
        bool Read();

        int Index { get; }

        int GetRowNumber(int index);

        IDeviceImportValueReader GetValue(int columnIndex);

        bool TestAllNotEmpty(int columnIndex);
        bool TestAllNullableInt(int columnIndex);
        bool TestAllInt(int columnIndex);
        bool TestAllNullableBool(int columnIndex);
        bool TestAllNullableDateTime(int columnIndex);
    }
}
