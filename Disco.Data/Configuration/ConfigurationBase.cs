using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Data.Configuration
{
    public abstract class ConfigurationBase
    {
        private ConfigurationContext _context;

        public ConfigurationContext Context
        {
            get
            {
                return _context;
            }
        }
        public abstract string Scope { get; }

        public ConfigurationBase(ConfigurationContext Context)
        {
            this._context = Context;
        }

        protected void SetValue<ValueType>(string Key, ValueType Value)
        {
            this.Context.SetConfigurationValue(this.Scope, Key, Value);
        }
        protected ValueType GetValue<ValueType>(string Key, ValueType Default)
        {
            return this.Context.GetConfigurationValue(this.Scope, Key, Default);
        }

    }
}
