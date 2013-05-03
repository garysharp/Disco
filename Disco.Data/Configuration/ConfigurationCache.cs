using Disco.Data.Repository;
using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Data.Configuration
{
    internal static class ConfigurationCache
    {
        #region Cache

        private static Dictionary<String, Dictionary<String, ConfigurationItem>> configDictionary = new Dictionary<string, Dictionary<string, ConfigurationItem>>();
        private static List<ConfigurationItem> configurationItems = new List<ConfigurationItem>();
        private static object configurationItemsLock = new object();

        private static void LoadConfigurationItems(DiscoDataContext dbContext, string Scope, bool Reload)
        {
            if (Reload || !configDictionary.ContainsKey(Scope))
            {
                lock (configurationItemsLock)
                {
                    if (Reload || !configDictionary.ContainsKey(Scope))
                    {
                        if (dbContext == null)
                            throw new InvalidOperationException("Cache-miss where Configuration Item requested from Cache-Only Configuration Context");

                        var newItems = dbContext.ConfigurationItems.Where(ci => ci.Scope == Scope).ToArray();

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
        private static Dictionary<string, Dictionary<string, ConfigurationItem>> ConfigurationDictionary(DiscoDataContext dbContext, string IncludingScope)
        {
            LoadConfigurationItems(dbContext, IncludingScope, false);
            return configDictionary;
        }
        private static ConfigurationItem ConfigurationItem(DiscoDataContext dbContext, string Scope, string Key)
        {
            Dictionary<string, ConfigurationItem> scopeDict = default(Dictionary<string, ConfigurationItem>);
            if (ConfigurationDictionary(dbContext, Scope).TryGetValue(Scope, out scopeDict))
            {
                ConfigurationItem item = default(ConfigurationItem);
                if (scopeDict.TryGetValue(Key, out item))
                    return item;
            }
            return null;
        }
        private static List<ConfigurationItem> ConfigurationItems(DiscoDataContext dbContext, string IncludingScope)
        {
            LoadConfigurationItems(dbContext, IncludingScope, false);
            return configurationItems;
        }

        #endregion

        #region Public Helpers
        internal static ValueType GetConfigurationValue<ValueType>(DiscoDataContext dbContext, string Scope, string Key, ValueType Default)
        {
            var ci = ConfigurationItem(dbContext, Scope, Key);
            if (ci == null)
                return Default;
            else
                return (ValueType)Convert.ChangeType(ci.Value, typeof(ValueType));
        }
        internal static void SetConfigurationValue<ValueType>(DiscoDataContext dbContext, string Scope, string Key, ValueType Value)
        {
            if (dbContext == null)
                throw new InvalidOperationException("Cannot save changes with a CacheOnly Context");

            var ci = ConfigurationItem(dbContext, Scope, Key);
            if (ci == null && Value != null)
            {
                lock (configurationItemsLock)
                {
                    ci = ConfigurationItem(dbContext, Scope, Key);
                    if (ci == null)
                    {
                        // Create Configuration Item
                        ci = new ConfigurationItem() { Scope = Scope, Key = Key, Value = Value.ToString() };
                        // Add Item to DB & Internal Collections
                        dbContext.ConfigurationItems.Add(ci);
                        ConfigurationItems(dbContext, Scope).Add(ci);
                        ConfigurationDictionary(dbContext, Scope)[Scope].Add(Key, ci);
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
                        LoadConfigurationItems(dbContext, Scope, true);
                        ci = ConfigurationItem(dbContext, Scope, Key);
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

        internal static List<ConfigurationItem> GetConfigurationItems(DiscoDataContext dbContext, string Scope)
        {
            return ConfigurationDictionary(dbContext, Scope)[Scope].Values.ToList();
        }

        internal static string ObsfucateValue(string Value)
        {
            if (string.IsNullOrEmpty(Value))
                return Value;
            else
                return Convert.ToBase64String(Encoding.Unicode.GetBytes(Value));
        }
        internal static string DeobsfucateValue(string ObsfucatedValue)
        {
            if (string.IsNullOrEmpty(ObsfucatedValue))
                return ObsfucatedValue;
            else
                return Encoding.Unicode.GetString(Convert.FromBase64String(ObsfucatedValue));
        }
        #endregion

    }
}
