using Disco.Data.Repository;
using Disco.Models.Services.Devices;

namespace Disco.Data.Configuration.Modules
{
    public class DevicesConfiguration : ConfigurationBase
    {
        public DevicesConfiguration(DiscoDataContext Database) : base(Database) { }

        public override string Scope { get; } = "Devices";

        public DeviceExportOptions LastExportOptions
        {
            get => Get(DeviceExportOptions.DefaultOptions());
            set => Set(value);
        }
    }
}
