using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Data.Configuration.Modules
{
    public class BootstrapperConfiguration : ConfigurationBase
    {
        public BootstrapperConfiguration(ConfigurationContext Context) : base(Context) { }

        public override string Scope
        {
            get { return "Bootstrapper"; }
        }

        public string MacSshUsername
        {
            get
            {
                return this.GetValue("MacSshUsername", "root");
            }
            set
            {
                this.SetValue("MacSshUsername", value);
            }
        }

        public string MacSshPassword
        {
            get
            {
                return ConfigurationContext.DeobsfucateValue(this.GetValue("MacSshPassword", string.Empty));
            }
            set
            {
                this.SetValue("MacSshPassword", ConfigurationContext.ObsfucateValue(value));
            }
        }
    }
}
