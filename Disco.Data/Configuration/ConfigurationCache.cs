using Disco.Data.Repository;
using Disco.Models.Repository;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Data.Configuration
{
    using ConfigurationCacheItemType = Tuple<ConfigurationItem, object>;
    using ConfigurationCacheScopeType = ConcurrentDictionary<string, Tuple<ConfigurationItem, object>>;
    using ConfigurationCacheType = ConcurrentDictionary<string, ConcurrentDictionary<string, Tuple<ConfigurationItem, object>>>;

    internal static class ConfigurationCache
    {
        #region Cache

        private static ConfigurationCacheType cacheStore = null;
        private static object configChangeLock = new object();

        private static ConfigurationCacheType Cache(DiscoDataContext Database)
        {
            if (ConfigurationCache.cacheStore == null)
            {
                lock (configChangeLock)
                {
                    if (ConfigurationCache.cacheStore == null)
                    {
                        if (Database == null)
                            throw new InvalidOperationException("The Configuration must be loaded at least once before a Cache-Only Configuration Context is used");

                        var configurationItems = Database.ConfigurationItems.ToArray();

                        var indexedItems = configurationItems
                            .GroupBy(ci => ci.Scope)
                            .Select(g =>
                                new KeyValuePair<string, ConfigurationCacheScopeType>(
                                    g.Key,
                                    new ConfigurationCacheScopeType(
                                        g.Select(i => new KeyValuePair<string, ConfigurationCacheItemType>(i.Key, Tuple.Create(i, (object)null))))));

                        cacheStore = new ConfigurationCacheType(indexedItems);
                    }
                }
            }

            return ConfigurationCache.cacheStore;
        }

        private static ConfigurationCacheItemType CacheGetItem(DiscoDataContext Database, string Scope, string Key)
        {
            var cache = Cache(Database);

            ConfigurationCacheScopeType scopeCache;
            if (cache.TryGetValue(Scope, out scopeCache))
            {
                ConfigurationCacheItemType item = default(ConfigurationCacheItemType);
                if (scopeCache.TryGetValue(Key, out item))
                    return item;
            }

            return null;
        }

        private static ConfigurationCacheItemType CacheSetItem(DiscoDataContext Database, string Scope, string Key, string Value, object ObjectValue)
        {
            if (Database == null)
                throw new InvalidOperationException("Cannot save changes with a Cache-Only Configuration Context");

            var item = CacheGetItem(Database, Scope, Key);

            if (item == null && Value == null)
            {
                // No Change - already null
                return null;
            }
            else if (item == null)
            {
                // New Configuration Item
                lock (configChangeLock)
                {
                    // Check again for thread safety
                    item = CacheGetItem(Database, Scope, Key);
                    if (item == null)
                    {
                        // Create Configuration Item
                        var configItem = new ConfigurationItem() { Scope = Scope, Key = Key, Value = Value };
                        item = new ConfigurationCacheItemType(configItem, ObjectValue);

                        // Add Item to DB
                        Database.ConfigurationItems.Add(configItem);

                        // Add Item to Cache
                        ConfigurationCacheScopeType scopeCache;
                        if (!cacheStore.TryGetValue(Scope, out scopeCache))
                        {
                            scopeCache = new ConfigurationCacheScopeType();
                            cacheStore.TryAdd(Scope, scopeCache);
                        }
                        scopeCache.TryAdd(Key, item);

                        return item;
                    }
                }
            }

            if (item != null)
            {
                // Existing Configuration Item
                lock (configChangeLock)
                {
                    var configItem = item.Item1;

                    // Compare Values
                    if (item.Item1.Value == Value)
                    {
                        // No Change - Update Cache Reference Only
                        return SetItemTypeValue(item, ObjectValue);
                    }
                    else
                    {
                        var entityInfo = Database.Entry(configItem);
                        if (entityInfo.State == System.Data.EntityState.Detached)
                        {
                            // Reload Item from DB
                            configItem = Database.ConfigurationItems.Where(i => i.Scope == Scope && i.Key == Key).First();
                        }

                        if (Value == null)
                        {
                            // Remove item from Database
                            Database.ConfigurationItems.Remove(configItem);

                            // Remove item from Cache
                            ConfigurationCacheScopeType scopeCache;
                            if (cacheStore.TryGetValue(Scope, out scopeCache))
                            {
                                scopeCache.TryRemove(Key, out item);
                            }

                            return null;
                        }
                        else
                        {
                            // Update Database
                            configItem.Value = Value;

                            // Update Cache
                            ConfigurationCacheScopeType scopeCache;
                            if (cacheStore.TryGetValue(Scope, out scopeCache))
                            {
                                scopeCache.TryRemove(Key, out item);
                                item = new ConfigurationCacheItemType(configItem, ObjectValue);
                                scopeCache.TryAdd(Key, item);
                                return item;
                            }
                        }
                    }
                }
            }

            return null;
        }
        private static ConfigurationCacheItemType SetItemTypeValue(ConfigurationCacheItemType ExistingItem, object Value)
        {
            var cache = ConfigurationCache.cacheStore;

            ConfigurationCacheScopeType scopeCache;
            if (cache.TryGetValue(ExistingItem.Item1.Scope, out scopeCache))
            {
                ConfigurationCacheItemType newItem = new ConfigurationCacheItemType(ExistingItem.Item1, Value);
                scopeCache.TryUpdate(ExistingItem.Item1.Key, newItem, ExistingItem);
                return newItem;
            }

            return null;
        }

        #endregion

        #region Helpers
        private static bool IsConvertableFromString(Type t)
        {
            if (t == typeof(Boolean) ||
                t == typeof(Char) ||
                t == typeof(SByte) ||
                t == typeof(Byte) ||
                t == typeof(Int16) || t == typeof(UInt16) ||
                t == typeof(Int32) || t == typeof(UInt32) ||
                t == typeof(Int64) || t == typeof(UInt64) ||
                t == typeof(Single) ||
                t == typeof(Double) ||
                t == typeof(Decimal) ||
                t == typeof(DateTime) ||
                t == typeof(String))
                return true;
            else
                return false;
        }
        #endregion

        #region Cache Getters/Setters
        internal static T GetValue<T>(DiscoDataContext Database, string Scope, string Key, T Default)
        {
            var item = CacheGetItem(Database, Scope, Key);

            if (item == null)
                return Default;
            else
            {
                if (item.Item2 != null && item.Item2.GetType() == typeof(T))
                {
                    // Return Cached Item
                    return (T)item.Item2;
                }
                else
                {
                    // Convert Serialized Item
                    Type itemType = typeof(T);
                    object itemValue;

                    if (itemType == typeof(string))
                    {
                        // string
                        itemValue = item.Item1.Value;
                    }
                    else if (itemType == typeof(object))
                    {
                        // object
                        itemValue = item.Item1.Value;
                    }
                    else if (IsConvertableFromString(itemType))
                    {
                        // IConvertable
                        itemValue = Convert.ChangeType(item.Item1.Value, itemType);
                    }
                    else if (itemType.BaseType != null && itemType.BaseType == typeof(Enum))
                    {
                        // Enum
                        itemValue = Enum.Parse(typeof(T), item.Item1.Value);
                    }
                    else
                    {
                        // JSON Deserialize
                        itemValue = JsonConvert.DeserializeObject<T>(item.Item1.Value);
                    }

                    // Set Item in Cache
                    SetItemTypeValue(item, itemValue);

                    return (T)itemValue;
                }
            }
        }
        internal static void SetValue<T>(DiscoDataContext Database, string Scope, string Key, T Value)
        {
            Type valueType = typeof(T);
            string stringValue;

            if (Value == null)
            {
                stringValue = null;
            }
            else if (valueType == typeof(object))
            {
                throw new ArgumentException(string.Format("Cannot serialize the configuration item [{0}].[{1}] which defines a type of [System.Object]", Scope, Key), "Value");
            }
            else if (IsConvertableFromString(valueType))
            {
                // string or supports IConvertable
                stringValue = Value.ToString();
            }
            else if (valueType.BaseType != null && valueType.BaseType == typeof(Enum))
            {
                // Enum
                stringValue = Value.ToString();
            }
            else
            {
                // JSON Serialize
                stringValue = JsonConvert.SerializeObject(Value);
            }

            CacheSetItem(Database, Scope, Key, stringValue, Value);
        }
        #endregion

        #region Cache Helpers

        internal static IEnumerable<string> GetScopeKeys(DiscoDataContext Database, string Scope)
        {
            var cache = Cache(Database);

            ConfigurationCacheScopeType scopeCache;
            if (cache.TryGetValue(Scope, out scopeCache))
            {
                return scopeCache.Keys.ToList();
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }

        internal static void RemoveScope(DiscoDataContext Database, string Scope)
        {
            if (Database == null)
                throw new InvalidOperationException("Cannot save changes with a Cache-Only Configuration Context");

            lock (configChangeLock)
            {
                // Remove item from Database
                var items = Database.ConfigurationItems.Where(i => i.Scope == Scope).ToList();
                items.ForEach(i => Database.ConfigurationItems.Remove(i));

                // Remove item from Cache
                if (cacheStore != null)
                {
                    ConfigurationCacheScopeType scopeCache;
                    cacheStore.TryRemove(Scope, out scopeCache);
                }
            }
        }

        internal static void RemoveScopeKey(DiscoDataContext Database, string Scope, string Key)
        {
            if (Database == null)
                throw new InvalidOperationException("Cannot save changes with a Cache-Only Configuration Context");

            lock (configChangeLock)
            {
                var cacheItem = CacheGetItem(Database, Scope, Key);
                ConfigurationItem configItem = null;

                // Remove item from Database
                if (cacheItem != null)
                {
                    configItem = cacheItem.Item1;

                    var entityInfo = Database.Entry(configItem);
                    if (entityInfo.State == System.Data.EntityState.Detached)
                    {
                        // Reload Item from DB
                        configItem = Database.ConfigurationItems.Where(i => i.Scope == Scope && i.Key == Key).FirstOrDefault();
                    }
                }
                if (configItem == null)
                {
                    // Load Item from DB
                    configItem = Database.ConfigurationItems.Where(i => i.Scope == Scope && i.Key == Key).FirstOrDefault();
                }
                if (configItem != null)
                {
                    Database.ConfigurationItems.Remove(configItem);
                }

                // Remove item from Cache
                if (cacheItem != null)
                {
                    ConfigurationCacheScopeType scopeCache;
                    if (cacheStore.TryGetValue(Scope, out scopeCache))
                    {
                        scopeCache.TryRemove(Key, out cacheItem);
                    }
                }
            }
        }

        #endregion

        #region Obsfucation Helpers
        internal static string Obsfucate(this string Value)
        {
            if (string.IsNullOrEmpty(Value))
                return Value;
            else
                return Convert.ToBase64String(Encoding.Unicode.GetBytes(Value));
        }
        internal static string Deobsfucate(this string ObsfucatedValue)
        {
            if (string.IsNullOrEmpty(ObsfucatedValue))
                return ObsfucatedValue;
            else
                return Encoding.Unicode.GetString(Convert.FromBase64String(ObsfucatedValue));
        }
        #endregion
    }
}
