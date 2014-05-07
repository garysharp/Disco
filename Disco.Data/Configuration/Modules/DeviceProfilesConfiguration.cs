using Disco.Data.Repository;
using System;

namespace Disco.Data.Configuration.Modules
{
    public class DeviceProfilesConfiguration : ConfigurationBase
    {
        public DeviceProfilesConfiguration(DiscoDataContext Database) : base(Database) { }

        public override string Scope { get { return "DeviceProfiles"; } }

        public int DefaultDeviceProfileId
        {
            get
            {
                var v = this.Get(1);
                if (v > 0)
                    return v;
                else
                    return 1;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value", "Expected >= 1");

                this.Set(value);
            }
        }
        public int DefaultAddDeviceOfflineDeviceProfileId
        {
            get
            {
                return this.Get(0);
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "Expected >= 0");

                this.Set(value);
            }
        }

    }
}
