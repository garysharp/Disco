using Disco.Data.Repository;
using Disco.Models.Repository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Disco.Data.Configuration
{
    public abstract class ConfigurationBase
    {
        private DiscoDataContext Database;

        public abstract string Scope { get; }

        public ConfigurationBase(DiscoDataContext Database)
        {
            this.Database = Database;
        }

        protected List<ConfigurationItem> Items
        {
            get
            {
                return ConfigurationCache.GetConfigurationItems(Database, this.Scope);
            }
        }

        private void SetValue<ValueType>(string Key, ValueType Value)
        {
            ConfigurationCache.SetConfigurationValue(Database, this.Scope, Key, Value);
        }
        private ValueType GetValue<ValueType>(string Key, ValueType Default)
        {
            return ConfigurationCache.GetConfigurationValue(Database, this.Scope, Key, Default);
        }

        protected void Set<ValueType>(ValueType Value, [CallerMemberName] string Key = null)
        {
            if (Key == null)
                throw new ArgumentNullException("Key");

            this.SetValue(Key, Value);
        }

        protected ValueType Get<ValueType>(ValueType Default, [CallerMemberName] string Key = null)
        {
            if (Key == null)
                throw new ArgumentNullException("Key");

            return this.GetValue(Key, Default);
        }

        protected void SetObsfucated(string Value, [CallerMemberName] string Key = null)
        {
            this.Set(ConfigurationCache.ObsfucateValue(Value), Key);
        }

        protected string GetDeobsfucated(string Default, [CallerMemberName] string Key = null)
        {
            var obsfucatedValue = this.Get<string>(null, Key);

            if (obsfucatedValue == null)
                return Default;
            else
                return ConfigurationCache.DeobsfucateValue(obsfucatedValue);
        }

        protected void SetAsJson<ValueType>(ValueType Value, [CallerMemberName] string Key = null)
        {
            if (Value == null)
                this.Set<string>(null, Key);
            else
                this.Set(JsonConvert.SerializeObject(Value), Key);
        }

        protected ValueType GetFromJson<ValueType>(ValueType Default, [CallerMemberName] string Key = null)
        {
            var jsonValue = this.Get<string>(null, Key);

            if (jsonValue == null)
                return Default;
            else
                return JsonConvert.DeserializeObject<ValueType>(jsonValue);
        }

        protected void SetAsEnum<EnumType>(EnumType Value, [CallerMemberName] string Key = null)
        {
            if (Value == null)
                this.Set<string>(null, Key);
            else
                this.Set(Value.ToString(), Key);
        }
        protected EnumType GetFromEnum<EnumType>(EnumType Default, [CallerMemberName] string Key = null)
        {
            var stringValue = this.Get<string>(null, Key);

            if (stringValue == null)
                return Default;
            else
                return (EnumType)Enum.Parse(typeof(EnumType), stringValue);
        }

    }
}
