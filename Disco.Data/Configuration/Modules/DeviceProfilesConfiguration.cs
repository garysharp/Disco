using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;

namespace Disco.Data.Configuration.Modules
{
    public class DeviceProfilesConfiguration : ConfigurationBase
    {
        // Removed 2012-06-14 G# - Properties moved to DeviceProfile model & DB Migrated in DBv3.
        //private Dictionary<int, DeviceProfileConfiguration> deviceProfileConfigurations;

        public DeviceProfilesConfiguration(ConfigurationContext Context)
            : base(Context)
        {
            // Removed 2012-06-14 G# - Properties moved to DeviceProfile model & DB Migrated in DBv3.
            //this.deviceProfileConfigurations = new Dictionary<int, DeviceProfileConfiguration>();
        }

        public override string Scope
        {
            get { return "DeviceProfiles"; }
        }

        // Removed 2012-06-14 G# - Properties moved to DeviceProfile model & DB Migrated in DBv3.
        //public DeviceProfileConfiguration DeviceProfile(DeviceProfile Profile)
        //{
        //    DeviceProfileConfiguration dpc = default(DeviceProfileConfiguration);
        //    if (!this.deviceProfileConfigurations.TryGetValue(Profile.Id, out dpc))
        //    {
        //        dpc = new DeviceProfileConfiguration(this.Context, Profile);
        //        this.deviceProfileConfigurations[Profile.Id] = dpc;
        //    }
        //    return dpc;
        //}

        public int DefaultDeviceProfileId
        {
            get
            {
                var v = this.GetValue("DefaultDeviceProfileId", 1);
                if (v > 0)
                    return v;
                else
                    return 1;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value", "Expected >= 1");
                this.SetValue("DefaultDeviceProfileId", value);
            }
        }
        public int DefaultAddDeviceOfflineDeviceProfileId
        {
            get
            {
                return this.GetValue("DefaultAddDeviceOfflineDeviceProfileId", 0);
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "Expected >= 0");
                this.SetValue("DefaultAddDeviceOfflineDeviceProfileId", value);
            }
        }

    }
}
