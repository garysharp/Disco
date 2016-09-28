using Disco.Data.Repository;
using Disco.Models.ClientServices;
using Disco.Models.ClientServices.EnrolmentInformation;
using Disco.Models.Repository;
using Disco.Services.Plugins.Features.WirelessProfileProvider;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services
{
    public static class DeviceWirelessProfileExtensions
    {

        public static WirelessProfileStore ProvisionWirelessProfiles(this Device device, DiscoDataContext Database, Enrol Enrolment)
        {
            var profiles = new List<Models.ClientServices.EnrolmentInformation.WirelessProfile>();
            var transformations = new List<Models.ClientServices.EnrolmentInformation.WirelessProfileTransformation>();
            var removeNames = new List<string>();

            foreach (var pluginFeature in device.DeviceProfile.GetWirelessProfileProviders())
            {
                using (var providerFeature = pluginFeature.CreateInstance<WirelessProfileProviderFeature>())
                {
                    var pluginResult = providerFeature.ProvisionWirelessProfiles(Database, device, Enrolment);

                    if (pluginResult.Profiles != null)
                    {
                        profiles.AddRange(pluginResult.Profiles.Select(p => new Models.ClientServices.EnrolmentInformation.WirelessProfile()
                        {
                            Name = p.Name,
                            ProfileXml = p.ProfileXml,
                            ForceDeployment = p.ForceDeployment
                        }));
                    }
                    if (pluginResult.Transformations != null)
                    {
                        transformations.AddRange(pluginResult.Transformations.Select(p => new Models.ClientServices.EnrolmentInformation.WirelessProfileTransformation()
                        {
                            Name = p.Name,
                            RegularExpression = p.RegularExpression,
                            RegularExpressionReplacement = p.RegularExpressionReplacement
                        }));
                    }
                    if (pluginResult.RemoveNames != null)
                    {
                        removeNames.AddRange(pluginResult.RemoveNames);
                    }
                }
            }

            if (profiles.Count == 0 && transformations.Count == 0 && removeNames.Count == 0)
            {
                return null;
            }
            else
            {
                return new WirelessProfileStore()
                {
                    Profiles = profiles.Count > 0 ? profiles : null,
                    Transformations = transformations.Count > 0 ? transformations : null,
                    RemoveNames = removeNames.Count > 0 ? removeNames : null
                };
            }
        }

    }
}
