using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Users.Contact;
using System.Collections.Generic;

namespace Disco.Services.Plugins.Features.DetailsProvider
{
    [PluginFeatureCategory(DisplayName = "User Contact Providers")]
    public abstract class UserContactFeature : PluginFeature
    {
        public abstract IEnumerable<UserContact> GetContacts(DiscoDataContext database, User user, UserContactType? contactType = null);
    }
}
