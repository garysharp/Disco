using Disco.Data.Repository;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Disco.Data.Configuration
{
    public abstract class ConfigurationBase
    {
        private DiscoDataContext Database;

        public abstract string Scope { get; }

        /// <summary>
        /// Creates a read-write configuration instance
        /// </summary>
        public ConfigurationBase(DiscoDataContext Database)
        {
            this.Database = Database;
        }

        /// <summary>
        /// Creates a read-only configuration instance
        /// </summary>
        public ConfigurationBase() : this(null) { }

        protected IEnumerable<string> ItemKeys
        {
            get
            {
                return ConfigurationCache.GetScopeKeys(Database, Scope);
            }
        }

        private void SetValue<T>(string Key, T Value)
        {
            ConfigurationCache.Helpers<T>.SetValue(Database, Scope, Key, Value);
        }
        private T GetValue<T>(string Key, T Default)
        {
            return ConfigurationCache.Helpers<T>.GetValue(Database, Scope, Key, Default);
        }

        protected void Set<T>(T Value, [CallerMemberName] string Key = null)
        {
            if (Key == null)
                throw new ArgumentNullException("Key");

            SetValue(Key, Value);
        }

        protected T Get<T>(T Default, [CallerMemberName] string Key = null)
        {
            if (Key == null)
                throw new ArgumentNullException("Key");

            return GetValue(Key, Default);
        }

        protected void SetObsfucated(string Value, [CallerMemberName] string Key = null)
        {
            Set(Value.Obsfucate(), Key);
        }

        protected string GetDeobsfucated(string Default, [CallerMemberName] string Key = null)
        {
            var obsfucatedValue = Get<string>(null, Key);

            if (obsfucatedValue == null)
                return Default;
            else
                return obsfucatedValue.Deobsfucate();
        }

        protected void RemoveScope()
        {
            ConfigurationCache.RemoveScope(Database, Scope);
        }

        protected void RemoveItem(string Key)
        {
            ConfigurationCache.RemoveScopeKey(Database, Scope, Key);
        }

        [Obsolete("Types are automatically serialized/deserialized if required, use Set<T>(T Value) instead.")]
        protected void SetAsJson<T>(T Value, [CallerMemberName] string Key = null)
        {
            Set(Value, Key);
        }

        [Obsolete("Types are automatically serialized/deserialized if required, use Get<T>(T Default) instead.")]
        protected T GetFromJson<T>(T Default, [CallerMemberName] string Key = null)
        {
            return Get(Default, Key);
        }

        [Obsolete("Types are automatically serialized/deserialized if required, use Set<T>(T Value) instead.")]
        protected void SetAsEnum<T>(T Value, [CallerMemberName] string Key = null)
        {
            Set(Value, Key);
        }
        [Obsolete("Types are automatically serialized/deserialized if required, use Set<T>(T Value) instead.")]
        protected T GetFromEnum<T>(T Default, [CallerMemberName] string Key = null)
        {
            return Get(Default, Key);
        }
    }
}
