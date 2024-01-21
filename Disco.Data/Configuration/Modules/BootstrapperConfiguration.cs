using Disco.Data.Repository;
using System;

namespace Disco.Data.Configuration.Modules
{
    public class BootstrapperConfiguration : ConfigurationBase
    {
        public BootstrapperConfiguration(DiscoDataContext Database) : base(Database) { }

        public override string Scope { get; } = "Bootstrapper";

        public string MacSshUsername
        {
            get => Get("root");
            set => Set(value);
        }

        public string MacSshPassword
        {
            get => GetDeobsfucated(string.Empty);
            set => SetObsfucated(value);
        }

        public TimeSpan PendingTimeout
        {
            get => TimeSpan.FromSeconds(Get(30 * 60)); // 30 minutes default
            set => Set((int)value.TotalSeconds);
        }
    }
}
