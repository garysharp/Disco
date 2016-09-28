using Disco.Data.Repository;
using Disco.Models.ClientServices;
using Disco.Models.Repository;

namespace Disco.Services.Plugins.Features.WirelessProfileProvider
{
    [PluginFeatureCategory(DisplayName = "Wireless Profile Providers")]
    public abstract class WirelessProfileProviderFeature : PluginFeature
    {

        public abstract string WirelessProfileProviderId { get; }

        public abstract ProvisionWirelessProfilesResult ProvisionWirelessProfiles(DiscoDataContext Database, Device Device, Enrol Enrolment);

    }
}
