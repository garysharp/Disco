using Disco.Data.Repository;
using Disco.Models.Services.Devices.Exporting;

namespace Disco.Data.Configuration.Modules
{
    public class DevicesConfiguration : ConfigurationBase
    {
        public DevicesConfiguration(DiscoDataContext Database) : base(Database) { }

        public override string Scope { get { return "Devices"; } }

        public DeviceExportOptions LastExportOptions
        {
            get { return this.Get<DeviceExportOptions>(DeviceExportOptions.DefaultOptions()); }
            set { this.Set(value); }
        }
    }
}
