using Disco.Data.Repository;
using Disco.Models.Services.Interop.DiscoServices;
using System;
using System.IO;

namespace Disco.Data.Configuration
{
    public class SystemConfiguration : ConfigurationBase
    {
        public SystemConfiguration(DiscoDataContext Database)
            : base(Database)
        {
            // Init Modules
            moduleBootstrapperConfiguration = new Lazy<Modules.BootstrapperConfiguration>(() => new Modules.BootstrapperConfiguration(Database));
            moduleDeviceProfilesConfiguration = new Lazy<Modules.DeviceProfilesConfiguration>(() => new Modules.DeviceProfilesConfiguration(Database));
            moduleOrganisationAddressesConfiguration = new Lazy<Modules.OrganisationAddressesConfiguration>(() => new Modules.OrganisationAddressesConfiguration(Database));
            moduleJobPreferencesConfiguration = new Lazy<Modules.JobPreferencesConfiguration>(() => new Modules.JobPreferencesConfiguration(Database));
            moduleActiveDirectoryConfiguration = new Lazy<Modules.ActiveDirectoryConfiguration>(() => new Modules.ActiveDirectoryConfiguration(Database));
            moduleDevicesConfiguration = new Lazy<Modules.DevicesConfiguration>(() => new Modules.DevicesConfiguration(Database));
            moduleDocumentsConfiguration = new Lazy<Modules.DocumentsConfiguration>(() => new Modules.DocumentsConfiguration(Database));
        }

        #region Configuration Modules

        private Lazy<Modules.BootstrapperConfiguration> moduleBootstrapperConfiguration;
        private Lazy<Modules.DeviceProfilesConfiguration> moduleDeviceProfilesConfiguration;
        private Lazy<Modules.OrganisationAddressesConfiguration> moduleOrganisationAddressesConfiguration;
        private Lazy<Modules.JobPreferencesConfiguration> moduleJobPreferencesConfiguration;
        private Lazy<Modules.ActiveDirectoryConfiguration> moduleActiveDirectoryConfiguration;
        private Lazy<Modules.DevicesConfiguration> moduleDevicesConfiguration;
        private Lazy<Modules.DocumentsConfiguration> moduleDocumentsConfiguration;

        public Modules.BootstrapperConfiguration Bootstrapper
        {
            get
            {
                return moduleBootstrapperConfiguration.Value;
            }
        }
        public Modules.DeviceProfilesConfiguration DeviceProfiles
        {
            get
            {
                return moduleDeviceProfilesConfiguration.Value;
            }
        }
        public Modules.OrganisationAddressesConfiguration OrganisationAddresses
        {
            get
            {
                return moduleOrganisationAddressesConfiguration.Value;
            }
        }
        public Modules.JobPreferencesConfiguration JobPreferences
        {
            get
            {
                return moduleJobPreferencesConfiguration.Value;
            }
        }
        public Modules.ActiveDirectoryConfiguration ActiveDirectory
        {
            get
            {
                return moduleActiveDirectoryConfiguration.Value;
            }
        }
        public Modules.DevicesConfiguration Devices
        {
            get
            {
                return moduleDevicesConfiguration.Value;
            }
        }

        public Modules.DocumentsConfiguration Documents
        {
            get
            {
                return moduleDocumentsConfiguration.Value;
            }
        }

        #endregion

        public override string Scope { get { return "System"; } }

        public string DataStoreLocation
        {
            get
            {
                var result = Get<string>(null);
                if (result == null)
                {
                    var appDataPath = System.Web.HttpContext.Current.Server.MapPath("~/App_Data");
                    if (appDataPath.EndsWith("\\"))
                        return appDataPath;
                    else
                        return string.Concat(appDataPath, '\\');
                }
                else
                    return result;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (!System.IO.Directory.Exists(value))
                    throw new System.IO.DirectoryNotFoundException(string.Format("DataStoreLocation: '{0}' could not be found", value));
                string storePath;
                if (value.EndsWith("\\"))
                    storePath = value;
                else
                    storePath = string.Concat(value, '\\');
                Set(storePath);
            }
        }

        public string Administrators
        {
            get
            {
                return Get("Domain Admins,Disco Admins");
            }
            set
            {
                Set(value);
            }
        }

        #region Plugin Locations
        public string PluginsLocation
        {
            get
            {
                return System.IO.Path.Combine(DataStoreLocation, @"Plugins\");
            }
        }
        public string PluginStorageLocation
        {
            get
            {
                return System.IO.Path.Combine(DataStoreLocation, @"PluginStorage\");
            }
        }
        public string PluginPackagesLocation
        {
            get
            {
                return System.IO.Path.Combine(DataStoreLocation, @"PluginPackages\");
            }
        }
        #endregion

        #region Organisation Details
        #region Organisation Logo
        private string OrganisationLogoPath
        {
            get
            {
                return System.IO.Path.Combine(DataStoreLocation, "OrganisationLogo.png");
            }
        }
        public string OrganisationLogoHash
        {
            get
            {
                var path = OrganisationLogoPath;
                if (File.Exists(path))
                    return File.GetLastWriteTimeUtc(path).ToBinary().ToString();
                else
                    return "-1";
            }
        }
        public Stream OrganisationLogo
        {
            get
            {
                var path = OrganisationLogoPath;
                if (File.Exists(path))
                    return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                else
                    return new MemoryStream(Disco.Data.Properties.Resources.EmptyLogo);
            }
            set
            {
                string organisationLogoPath = OrganisationLogoPath;
                if (value == null)
                {
                    if (System.IO.File.Exists(organisationLogoPath))
                        System.IO.File.Delete(organisationLogoPath);
                }
                else
                {
                    using (FileStream fs = new FileStream(organisationLogoPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        value.CopyTo(fs);
                    }
                }
            }
        }
        #endregion
        public string OrganisationName
        {
            get
            {
                return Get<string>(null);
            }
            set
            {
                Set(value);
            }
        }
        public bool MultiSiteMode
        {
            get
            {
                return Get(false);
            }
            set
            {
                Set(value);
            }
        }
        #endregion

        #region Proxy Configuration
        public string ProxyAddress
        {
            get
            {
                return Get<string>(null);
            }
            set
            {
                Set(value);
            }
        }
        public int ProxyPort
        {
            get
            {
                return Get(8080);
            }
            set
            {
                Set(value);
            }
        }
        public string ProxyUsername
        {
            get
            {
                return Get<string>(null);
            }
            set
            {
                Set(value);
            }
        }
        public string ProxyPassword
        {
            get
            {
                return GetDeobsfucated(null);
            }
            set
            {
                SetObsfucated(value);
            }
        }
        #endregion

        #region UpdateCheck
        public string DeploymentId
        {
            get
            {
                return Get<string>(null);
            }
        }
        public string DeploymentSecret
        {
            get
            {
                return Get<string>(null);
            }
        }
        public short DeploymentChecksum
        {
            get
            {
                var deploymentIdBytes = Guid.Parse(DeploymentId).ToByteArray();
                return
                    (short)(BitConverter.ToInt16(deploymentIdBytes, 0) ^
                    BitConverter.ToInt16(deploymentIdBytes, 2) ^
                    BitConverter.ToInt16(deploymentIdBytes, 4) ^
                    BitConverter.ToInt16(deploymentIdBytes, 6) ^
                    BitConverter.ToInt16(deploymentIdBytes, 8) ^
                    BitConverter.ToInt16(deploymentIdBytes, 10) ^
                    BitConverter.ToInt16(deploymentIdBytes, 12) ^
                    BitConverter.ToInt16(deploymentIdBytes, 14));
            }
        }
        public UpdateResponseV2 UpdateLastCheckResponse
        {
            get
            {
                return Get<UpdateResponseV2>(null);
            }
            set
            {
                Set(value);
            }
        }
        public bool UpdateBetaDeployment
        {
            get
            {
                return Get(false);
            }
        }
        public Version InstalledDatabaseVersion
        {
            get
            {
                var versionString = Get<string>(null);
                if (string.IsNullOrEmpty(versionString))
                    return null;
                else
                    return Version.Parse(versionString);
            }
            set
            {
                Set(value.ToString(4));
            }
        }
        #endregion

    }
}
