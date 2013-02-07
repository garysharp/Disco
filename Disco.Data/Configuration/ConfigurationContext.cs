using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Data.Repository;
using Disco.Models.Repository;
using System.IO;
using System.Security.Cryptography;
using Disco.Models.BI.Interop.Community;
using Newtonsoft.Json;

namespace Disco.Data.Configuration
{
    public class ConfigurationContext
    {
        private DiscoDataContext _dbContext;
        private DiscoDataContext dbContext
        {
            get
            {
                if (_dbContext != null)
                    return _dbContext;
                else
                    throw new InvalidOperationException("Cache-miss where Configuration Item requested from Cache-Only Configuration Context");
            }
        }

        public bool CacheOnly
        {
            get
            {
                return _dbContext == null;
            }
        }

        public ConfigurationContext(DiscoDataContext dbContext)
        {
            this._dbContext = dbContext;

            // Init Modules
            this.moduleBootstrapperConfiguration = new Lazy<Modules.BootstrapperConfiguration>(() => new Modules.BootstrapperConfiguration(this));
            this.moduleDeviceProfilesConfiguration = new Lazy<Modules.DeviceProfilesConfiguration>(() => new Modules.DeviceProfilesConfiguration(this));
            this.moduleOrganisationAddressesConfiguration = new Lazy<Modules.OrganisationAddressesConfiguration>(() => new Modules.OrganisationAddressesConfiguration(this));
            this.moduleWirelessConfiguration = new Lazy<Modules.WirelessConfiguration>(() => new Modules.WirelessConfiguration(this));
        }

        #region Item Cache

        private static Dictionary<String, Dictionary<String, ConfigurationItem>> configDictionary = new Dictionary<string, Dictionary<string, ConfigurationItem>>();
        private static List<ConfigurationItem> configurationItems = new List<ConfigurationItem>();
        private static object configurationItemsLock = new object();

        private void loadConfigurationItems(string Scope, bool Reload)
        {
            if (Reload || !configDictionary.ContainsKey(Scope))
            {
                lock (configurationItemsLock)
                {
                    if (Reload || !configDictionary.ContainsKey(Scope))
                    {
                        var newItems = this.dbContext.ConfigurationItems.Where(ci => ci.Scope == Scope).ToArray();

                        if (configDictionary.ContainsKey(Scope))
                        {
                            var existingItems = configDictionary[Scope];
                            foreach (var existingItem in existingItems.Values)
                            {
                                configurationItems.Remove(existingItem);
                            }
                        }
                        configurationItems.AddRange(newItems);
                        configDictionary[Scope] = newItems.ToDictionary(ci => ci.Key);
                    }
                }
            }
        }
        public Dictionary<string, Dictionary<string, ConfigurationItem>> ConfigurationDictionary(string IncludingScope)
        {
            this.loadConfigurationItems(IncludingScope, false);
            return configDictionary;
        }
        public ConfigurationItem ConfigurationItem(string Scope, string Key)
        {
            Dictionary<string, ConfigurationItem> scopeDict = default(Dictionary<string, ConfigurationItem>);
            if (this.ConfigurationDictionary(Scope).TryGetValue(Scope, out scopeDict))
            {
                ConfigurationItem item = default(ConfigurationItem);
                if (scopeDict.TryGetValue(Key, out item))
                    return item;
            }
            return null;
        }
        private List<ConfigurationItem> ConfigurationItems(string IncludingScope)
        {
            this.loadConfigurationItems(IncludingScope, false);
            return configurationItems;
        }

        #endregion

        #region Helpers
        public ValueType GetConfigurationValue<ValueType>(string Scope, string Key, ValueType Default)
        {
            var ci = this.ConfigurationItem(Scope, Key);
            if (ci == null)
                return Default;
            else
                return (ValueType)Convert.ChangeType(ci.Value, typeof(ValueType));
        }
        public void SetConfigurationValue<ValueType>(string Scope, string Key, ValueType Value)
        {
            if (CacheOnly)
                throw new InvalidOperationException("Cannot save changes with a CacheOnly Context");

            var ci = this.ConfigurationItem(Scope, Key);
            if (ci == null && Value != null)
            {
                lock (configurationItemsLock)
                {
                    ci = this.ConfigurationItem(Scope, Key);
                    if (ci == null)
                    {
                        // Create Configuration Item
                        ci = new ConfigurationItem() { Scope = Scope, Key = Key, Value = Value.ToString() };
                        // Add Item to DB & Internal Collections
                        this.dbContext.ConfigurationItems.Add(ci);
                        this.ConfigurationItems(Scope).Add(ci);
                        this.ConfigurationDictionary(Scope)[Scope].Add(Key, ci);
                        ci = null;
                    }
                }
            }
            if (ci != null)
            {
                lock (configurationItemsLock)
                {
                    var entityInfo = dbContext.Entry(ci);
                    if (entityInfo.State == System.Data.EntityState.Detached)
                    {
                        // Reload Scope from DB
                        this.loadConfigurationItems(Scope, true);
                        ci = this.ConfigurationItem(Scope, Key);
                    }

                    if (Value == null)
                    {
                        dbContext.ConfigurationItems.Remove(ci);
                        configurationItems.Remove(ci);
                        configDictionary[Scope].Remove(Key);
                    }
                    else
                    {
                        ci.Value = Value.ToString();
                    }
                }
            }
        }

        public static string ObsfucateValue(string Value)
        {
            if (string.IsNullOrEmpty(Value))
                return Value;
            else
                return Convert.ToBase64String(Encoding.Unicode.GetBytes(Value));
        }
        public static string DeobsfucateValue(string ObsfucatedValue)
        {
            if (string.IsNullOrEmpty(ObsfucatedValue))
                return ObsfucatedValue;
            else
                return Encoding.Unicode.GetString(Convert.FromBase64String(ObsfucatedValue));
        }
        #endregion

        #region Configuration Modules

        private Lazy<Modules.BootstrapperConfiguration> moduleBootstrapperConfiguration;
        private Lazy<Modules.DeviceProfilesConfiguration> moduleDeviceProfilesConfiguration;
        private Lazy<Modules.OrganisationAddressesConfiguration> moduleOrganisationAddressesConfiguration;
        private Lazy<Modules.WirelessConfiguration> moduleWirelessConfiguration;

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
        public Modules.WirelessConfiguration Wireless
        {
            get
            {
                return moduleWirelessConfiguration.Value;
            }
        }

        #endregion

        #region System Configuration Items

        public string Scope { get { return "System"; } }

        public string DataStoreLocation
        {
            get
            {
                var result = this.GetConfigurationValue<string>(this.Scope, "DataStoreLocation", null);
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
                this.SetConfigurationValue(this.Scope, "DataStoreLocation", storePath);
            }
        }
        public string PluginsLocation
        {
            get
            {
                return System.IO.Path.Combine(this.DataStoreLocation, @"Plugins\");
            }
        }
        public string PluginStorageLocation
        {
            get
            {
                return System.IO.Path.Combine(this.DataStoreLocation, @"PluginStorage\");
            }
        }
        public string PluginPackagesLocation
        {
            get
            {
                return System.IO.Path.Combine(this.DataStoreLocation, @"PluginPackages\");
            }
        }
        #region Organisation Logo
        private string OrganisationLogoPath
        {
            get
            {
                return System.IO.Path.Combine(DataStoreLocation, "OrganisationLogo.png");
            }
        }
        //private static string _OrganisationLogoHash;
        //private static byte[] _OrganisationLogo;
        //private static object _OrganisationLogoLock = new object();
        //private static void LoadOrganisationLogo(ConfigurationContext context, bool reload = false)
        //{
        //    if (_OrganisationLogoHash == null || reload)
        //    {
        //        lock (_OrganisationLogoLock)
        //        {
        //            if (_OrganisationLogoHash == null || reload)
        //            {
        //                _OrganisationLogo = null;
        //                _OrganisationLogoHash = null;

        //                string organisationLogoPath = context.OrganisationLogoPath;
        //                if (System.IO.File.Exists(organisationLogoPath))
        //                    _OrganisationLogo = System.IO.File.ReadAllBytes(organisationLogoPath);
        //            }
        //            if (_OrganisationLogo == null || _OrganisationLogo.Length == 0)
        //            {
        //                _OrganisationLogo = Disco.Data.Properties.Resources.EmptyLogo;
        //            }
        //            if (_OrganisationLogoHash == null)
        //            {
        //                using (SHA256 h = SHA256.Create())
        //                {
        //                    _OrganisationLogoHash = Convert.ToBase64String(h.ComputeHash(_OrganisationLogo));
        //                }
        //            }
        //        }
        //    }
        //}
        public string OrganisationLogoHash
        {
            get
            {
                var path = this.OrganisationLogoPath;
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
                //LoadOrganisationLogo(this);
                //if (_OrganisationLogo == null || _OrganisationLogo.Length != 0)
                //    return new MemoryStream(_OrganisationLogo);
                //else
                //    return null;
                var path = this.OrganisationLogoPath;
                if (File.Exists(path))
                    return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                else
                    return new MemoryStream(Disco.Data.Properties.Resources.EmptyLogo);
            }
            set
            {
                string organisationLogoPath = this.OrganisationLogoPath;
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
                //LoadOrganisationLogo(this, true);
            }
        }
        #endregion
        public string OrganisationName
        {
            get
            {
                return this.GetConfigurationValue<string>(this.Scope, "OrganisationName", null);
            }
            set
            {
                this.SetConfigurationValue(this.Scope, "OrganisationName", value);
            }
        }
        public bool MultiSiteMode
        {
            get
            {
                return this.GetConfigurationValue<bool>(this.Scope, "MultiSiteMode", false);
            }
            set
            {
                this.SetConfigurationValue(this.Scope, "MultiSiteMode", value);
            }
        }

        #region Proxy Configuration
        public string ProxyAddress
        {
            get
            {
                return this.GetConfigurationValue<string>(this.Scope, "ProxyAddress", null);
            }
            set
            {
                this.SetConfigurationValue(this.Scope, "ProxyAddress", value);
            }
        }
        public int ProxyPort
        {
            get
            {
                return this.GetConfigurationValue(this.Scope, "ProxyPort", 8080);
            }
            set
            {
                this.SetConfigurationValue(this.Scope, "ProxyPort", value);
            }
        }
        public string ProxyUsername
        {
            get
            {
                return this.GetConfigurationValue<string>(this.Scope, "ProxyUsername", null);
            }
            set
            {
                this.SetConfigurationValue(this.Scope, "ProxyUsername", value);
            }
        }
        public string ProxyPassword
        {
            get
            {
                return DeobsfucateValue(this.GetConfigurationValue<string>(this.Scope, "ProxyPassword", null));
            }
            set
            {
                this.SetConfigurationValue(this.Scope, "ProxyPassword", ObsfucateValue(value));
            }
        }
        #endregion

        #region UpdateCheck
        public string DeploymentId
        {
            get
            {
                return this.GetConfigurationValue<string>(this.Scope, "DeploymentId", null);
            }
        }
        public UpdateResponse UpdateLastCheck
        {
            get
            {
                var json = this.GetConfigurationValue<string>(this.Scope, "UpdateLastCheck", null);
                if (json != null)
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<UpdateResponse>(json);
                    }
                    catch (Exception)
                    { }// Ignore Serialization Issues
                }
                return null;
            }
            set
            {
                if (value == null)
                    this.SetConfigurationValue<string>(this.Scope, "UpdateLastCheck", null);

                var json = JsonConvert.SerializeObject(value);
                this.SetConfigurationValue<string>(this.Scope, "UpdateLastCheck", json);
            }
        }
        public bool UpdateBetaDeployment
        {
            get
            {
                return this.GetConfigurationValue<bool>(this.Scope, "UpdateBetaDeployment", false);
            }
        }
        public string InstalledDatabaseVersion
        {
            get
            {
                return this.GetConfigurationValue<string>(this.Scope, "InstalledDatabaseVersion", null);
            }
            set
            {
                this.SetConfigurationValue<string>(this.Scope, "InstalledDatabaseVersion", value);
            }
        }
        #endregion

        #endregion

    }
}
