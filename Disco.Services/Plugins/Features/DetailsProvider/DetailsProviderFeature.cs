using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Plugins.Details;
using System;

namespace Disco.Services.Plugins.Features.DetailsProvider
{
    [PluginFeatureCategory(DisplayName = "Detail Providers")]
    public abstract class DetailsProviderFeature : PluginFeature
    {
        public abstract DetailsResult GetDetails(DiscoDataContext database, User user, DateTime? cacheTimestamp);
        public abstract DetailsResult GetDetails(DiscoDataContext database, Device device, DateTime? cacheTimestamp);
        public abstract byte[] GetUserPhoto(DiscoDataContext database, User user, DateTime? cacheTimestamp);
    }
}
