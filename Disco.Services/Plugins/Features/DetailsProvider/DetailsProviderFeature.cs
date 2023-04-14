using Disco.Data.Repository;
using Disco.Models.Repository;
using System;

namespace Disco.Services.Plugins.Features.DetailsProvider
{
    [PluginFeatureCategory(DisplayName = "Detail Providers")]
    public abstract class DetailsProviderFeature : PluginFeature
    {
        public abstract void UpdateAllDetails(DiscoDataContext database);

        public abstract byte[] GetUserPhoto(DiscoDataContext database, User user, DateTime? cacheTimestamp);
    }
}
