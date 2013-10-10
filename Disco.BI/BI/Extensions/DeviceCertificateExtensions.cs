using System.Linq;
using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.CertificateProvider;
using System;
using System.Collections.Generic;

namespace Disco.BI.Extensions
{
    public static class DeviceCertificateExtensions
    {

        public static Tuple<DeviceCertificate, List<string>> AllocateCertificate(this Device device, DiscoDataContext Database)
        {
            if (!string.IsNullOrEmpty(device.DeviceProfile.CertificateProviderId))
            {
                // Load Plugin
                PluginFeatureManifest featureManifest = Plugins.GetPluginFeature(device.DeviceProfile.CertificateProviderId, typeof(CertificateProviderFeature));

                using (CertificateProviderFeature providerFeature = featureManifest.CreateInstance<CertificateProviderFeature>())
                {
                    // REMOVED 2012-07-18 G# - Plugin is responsible for checking
                    // Already Allocated Certificate
                    //if (deviceCertificates.Count > 0)
                    //    return new Tuple<DeviceCertificate, List<string>>(deviceCertificates[0], providerPlugin.RemoveExistingCertificateNames());
                    //else

                    return providerFeature.AllocateCertificate(Database, device);    
                }
            }

            // Device Profile does not allow certificate allocation
            return null;
        }

    }
}
