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
        private DiscoDataContext dbContext;

        public abstract string Scope { get; }

        public ConfigurationBase(DiscoDataContext dbContext)
        {
            this.dbContext = dbContext;
        }

        protected List<ConfigurationItem> Items
        {
            get
            {
                return ConfigurationCache.GetConfigurationItems(dbContext, this.Scope);
            }
        }

        private void SetValue<ValueType>(string Key, ValueType Value)
        {
            ConfigurationCache.SetConfigurationValue(dbContext, this.Scope, Key, Value);
        }
        private ValueType GetValue<ValueType>(string Key, ValueType Default)
        {
            return ConfigurationCache.GetConfigurationValue(dbContext, this.Scope, Key, Default);
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

    }
}
