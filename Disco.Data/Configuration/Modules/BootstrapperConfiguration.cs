using Disco.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Data.Configuration.Modules
{
    public class BootstrapperConfiguration : ConfigurationBase
    {
        public BootstrapperConfiguration(DiscoDataContext dbContext) : base(dbContext) { }

        public override string Scope
        {
            get { return "Bootstrapper"; }
        }

        public string MacSshUsername
        {
            get
            {
                return this.Get("root");
            }
            set
            {
                this.Set(value);
            }
        }

        public string MacSshPassword
        {
            get
            {
                return this.GetDeobsfucated(string.Empty);
            }
            set
            {
                this.SetObsfucated(value);
            }
        }
    }
}
