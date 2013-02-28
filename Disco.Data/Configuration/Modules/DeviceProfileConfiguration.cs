// Removed 2012-06-14 G# - Properties moved to DeviceProfile model & DB Migrated in DBv3.
//
//
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Disco.Models.Repository;

//namespace Disco.Data.Configuration.Modules
//{
//    public class DeviceProfileConfiguration : ConfigurationBase
//    {
//        private DeviceProfilesConfiguration deviceProfilesConfig;
//        private DeviceProfile deviceProfile;

//        public DeviceProfileConfiguration(ConfigurationContext Context, DeviceProfile DeviceProfile)
//            : base(Context)
//        {
//            this.deviceProfilesConfig = Context.DeviceProfiles;
//            this.deviceProfile = DeviceProfile;
//        }

//        public override string Scope
//        {
//            get
//            {
//                return string.Format("DeviceProfile:{0}", this.deviceProfile.Id);
//            }
//        }

//        public string ComputerNameTemplate
//        {
//            get
//            {
//                return this.GetValue("ComputerNameTemplate", "DeviceProfile.ShortName + '-' + SerialNumber");
//            }
//            set
//            {
//                this.SetValue("ComputerNameTemplate", value);
//            }
//        }

//        public enum DeviceProfileDistributionTypes : int
//        {
//            OneToMany = 0,
//            OneToOne = 1
//        }
//        public DeviceProfileDistributionTypes DistributionType
//        {
//            get
//            {
//                return (DeviceProfileDistributionTypes)this.GetValue("DistributionType", (int)DeviceProfileDistributionTypes.OneToMany);
//            }
//            set
//            {
//                this.SetValue("DistributionType", (int)value);
//            }
//        }
//        public string OrganisationalUnit
//        {
//            get
//            {
//                return this.GetValue<string>("OrganisationalUnit", null);
//            }
//            set
//            {
//                this.SetValue("OrganisationalUnit", value);
//            }
//        }
//        public bool AllocateWirelessCertificate
//        {
//            get
//            {
//                return this.GetValue("AllocateWirelessCertificate", false);
//            }
//            set
//            {
//                this.SetValue("AllocateWirelessCertificate", value);
//            }
//        }

        

//    }
//}
