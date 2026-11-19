using Disco.Data.Repository;
using Disco.Models.Services.Devices;
using Disco.Models.Services.Devices.Importing;

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

        public bool EnrollmentLegacyDiscoveryDisabled
        {
            get => Get(false);
            set => Set(value);
        }

        public IntuneDeviceImportConfiguration LastIntuneImportConfiguration
        {
            get => Get(new IntuneDeviceImportConfiguration());
            set => Set(value);
        }
    }
}
