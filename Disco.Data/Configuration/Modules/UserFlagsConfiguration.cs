using Disco.Data.Repository;
using Disco.Models.Services.Users.UserFlags;

namespace Disco.Data.Configuration.Modules
{
    public class UserFlagsConfiguration : ConfigurationBase
    {
        public UserFlagsConfiguration(DiscoDataContext database) : base(database) { }

        public override string Scope { get; } = "UserFlags";

        public UserFlagExportOptions LastExportOptions
        {
            get => Get(UserFlagExportOptions.DefaultOptions());
            set => Set(value);
        }
    }
}
