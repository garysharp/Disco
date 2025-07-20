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
            if (cacheStore == null)
            {
                lock (configChangeLock)
                {
                    if (cacheStore == null)
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

            return cacheStore;
        }

        private static ConfigurationCacheItemType CacheGetItem(DiscoDataContext Database, string Scope, string Key)
        {
            var cache = Cache(Database);

            if (cache.TryGetValue(Scope, out var scopeCache))
            {
                if (scopeCache.TryGetValue(Key, out var item))
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
                        if (!cacheStore.TryGetValue(Scope, out var scopeCache))
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
                            if (cacheStore.TryGetValue(Scope, out var scopeCache))
                            {
                                scopeCache.TryRemove(Key, out _);
                            }

                            return null;
                        }
                        else
                        {
                            // Update Database
                            configItem.Value = Value;

                            // Update Cache
                            if (cacheStore.TryGetValue(Scope, out var scopeCache))
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
            var cache = cacheStore;

            if (cache.TryGetValue(ExistingItem.Item1.Scope, out var scopeCache))
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
        internal static class Helpers<T>
        {
            private static readonly IEqualityComparer<T> comparer = EqualityComparer<T>.Default;

            internal static T GetValue(DiscoDataContext Database, string Scope, string Key, T Default)
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
                            // enum
                            itemValue = Enum.Parse(typeof(T), item.Item1.Value);
                        }
                        else if (itemType.IsGenericType &&
                            itemType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                            IsConvertableFromString(Nullable.GetUnderlyingType(itemType)))
                        {
                            // nullable
                            itemValue = (T)Convert.ChangeType(item.Item1.Value, Nullable.GetUnderlyingType(itemType));
                        }
                        else if (itemType == typeof(Guid))
                        {
                            // guid
                            itemValue = new Guid(item.Item1.Value);
                        }
                        else if (itemType == typeof(Guid?))
                        {
                            // guid
                            if (string.IsNullOrEmpty(item.Item1.Value))
                                itemValue = null;
                            else
                                itemValue = new Guid(item.Item1.Value);
                        }
                        else if (itemType == typeof(byte[]))
                        {
                            // byte[]
                            itemValue = Convert.FromBase64String(item.Item1.Value);
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
            internal static void SetValue(DiscoDataContext Database, string Scope, string Key, T Value)
            {
                Type valueType = typeof(T);
                string stringValue;

                if (comparer.Equals(Value, default))
                {
                    stringValue = null;
                }
                else if (valueType == typeof(object))
                {
                    throw new ArgumentException($"Cannot serialize the configuration item [{Scope}].[{Key}] which has the type [System.Object]", "Value");
                }
                else if (IsConvertableFromString(valueType))
                {
                    // string or supports IConvertable
                    stringValue = Value.ToString();
                }
                else if (valueType.BaseType != null && valueType.BaseType == typeof(Enum))
                {
                    // enum
                    stringValue = Value.ToString();
                }
                else if (valueType.IsGenericType &&
                    valueType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                    IsConvertableFromString(Nullable.GetUnderlyingType(valueType)))
                {
                    // nullable
                    stringValue = Value.ToString();
                }
                else if (valueType == typeof(Guid) || valueType == typeof(Guid?))
                {
                    stringValue = Value.ToString();
                }
                else if (Value is byte[] valueBytes)
                {
                    // byte[]
                    stringValue = Convert.ToBase64String(valueBytes);
                }
                else
                {
                    // JSON Serialize
                    stringValue = JsonConvert.SerializeObject(Value);
                }

                CacheSetItem(Database, Scope, Key, stringValue, Value);
            }
        }

        #endregion

        #region Cache Helpers

        internal static IEnumerable<string> GetScopeKeys(DiscoDataContext Database, string Scope)
        {
            var cache = Cache(Database);

            if (cache.TryGetValue(Scope, out var scopeCache))
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
                    cacheStore.TryRemove(Scope, out var scopeCache);
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
                    if (cacheStore.TryGetValue(Scope, out var scopeCache))
                    {
                        scopeCache.TryRemove(Key, out _);
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
