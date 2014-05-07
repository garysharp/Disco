using Disco.Data.Repository;

namespace Disco.Data.Configuration.Modules
{
    public class BootstrapperConfiguration : ConfigurationBase
    {
        public BootstrapperConfiguration(DiscoDataContext Database) : base(Database) { }

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
