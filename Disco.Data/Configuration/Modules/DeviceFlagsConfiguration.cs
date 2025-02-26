using Disco.Data.Repository;
using Disco.Models.Services.Devices.DeviceFlag;

namespace Disco.Data.Configuration.Modules
{
    public class DeviceFlagsConfiguration : ConfigurationBase
    {
        public DeviceFlagsConfiguration(DiscoDataContext database) : base(database) { }

        public override string Scope { get; } = "DeviceFlags";

        public DeviceFlagExportOptions LastExportOptions
        {
            get => Get(DeviceFlagExportOptions.DefaultOptions());
            set => Set(value);
        }
    }
}
