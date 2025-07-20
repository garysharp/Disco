using Disco.Data.Repository;
using Disco.Models.BI.Config;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Devices.ManagedGroups;
using Disco.Services.Expressions;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.CertificateAuthorityProvider;
using Disco.Services.Plugins.Features.CertificateProvider;
using Disco.Services.Plugins.Features.WirelessProfileProvider;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services
{
    public static class DeviceProfileExtensions
    {
        public const string ComputerNameExpressionCacheTemplate = "ComputerNameTemplate_{0}";

        public static void ComputerNameInvalidateCache(this DeviceProfile deviceProfile)
        {
            ExpressionCache.InvalidateSingleCache(string.Format(ComputerNameExpressionCacheTemplate, deviceProfile.Id));
        }

        public static OrganisationAddress DefaultOrganisationAddressDetails(this DeviceProfile deviceProfile, DiscoDataContext Database)
        {
            if (deviceProfile.DefaultOrganisationAddress.HasValue)
            {
                return Database.DiscoConfiguration.OrganisationAddresses.GetAddress(deviceProfile.DefaultOrganisationAddress.Value);
            }
            else
            {
                return null;
            }
        }

        public static bool CanDelete(this DeviceProfile dp, DiscoDataContext Database)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Config.DeviceProfile.Delete))
                return false;

            // Can't Delete Default Profile (Id: 1)
            if (dp.Id == 1)
                return false;

            // Can't Delete if Contains Devices
            if (Database.Devices.Count(d => d.DeviceProfileId == dp.Id) > 0)
                return false;

            return true;
        }
        public static void Delete(this DeviceProfile dp, DiscoDataContext Database)
        {
            if (!dp.CanDelete(Database))
                throw new InvalidOperationException("The state of this Device Profile doesn't allow it to be deleted");

            // Update Defaults
            if (Database.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId == dp.Id)
                Database.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId = 1;
            if (Database.DiscoConfiguration.DeviceProfiles.DefaultAddDeviceOfflineDeviceProfileId == dp.Id)
                Database.DiscoConfiguration.DeviceProfiles.DefaultAddDeviceOfflineDeviceProfileId = 1;

            // Remove Linked Group
            ActiveDirectory.Context.ManagedGroups.Remove(DeviceProfileDevicesManagedGroup.GetKey(dp));
            ActiveDirectory.Context.ManagedGroups.Remove(DeviceProfileAssignedUsersManagedGroup.GetKey(dp));

            // Delete Profile
            Database.DeviceProfiles.Remove(dp);
        }

        public static bool CanDecommission(this DeviceProfile dp, DiscoDataContext database)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Device.Actions.Import))
                return false;

            if (!database.Devices.Any(d => d.DeviceProfileId == dp.Id && d.DecommissionedDate == null))
                return false;

            return true;
        }

        public static IEnumerable<PluginFeatureManifest> GetCertificateProviders(this DeviceProfile dp)
        {
            if (!string.IsNullOrEmpty(dp.CertificateProviders))
            {
                foreach (var certificateProviderId in dp.CertificateProviders.Split(','))
                {
                    if (Plugins.Plugins.TryGetPluginFeature(certificateProviderId.Trim(), typeof(CertificateProviderFeature), out var featureManifest))
                    {
                        yield return featureManifest;
                    }
                }
            }
        }

        public static IEnumerable<PluginFeatureManifest> GetCertificateAuthorityProviders(this DeviceProfile dp)
        {
            if (!string.IsNullOrEmpty(dp.CertificateAuthorityProviders))
            {
                foreach (var certificateAuthorityProviderId in dp.CertificateAuthorityProviders.Split(','))
                {
                    if (Plugins.Plugins.TryGetPluginFeature(certificateAuthorityProviderId.Trim(), typeof(CertificateAuthorityProviderFeature), out var featureManifest))
                    {
                        yield return featureManifest;
                    }
                }
            }
        }

        public static IEnumerable<PluginFeatureManifest> GetWirelessProfileProviders(this DeviceProfile dp)
        {
            if (!string.IsNullOrEmpty(dp.WirelessProfileProviders))
            {
                foreach (var wirelessProfileProviderId in dp.WirelessProfileProviders.Split(','))
                {
                    if (Plugins.Plugins.TryGetPluginFeature(wirelessProfileProviderId.Trim(), typeof(WirelessProfileProviderFeature), out var featureManifest))
                    {
                        yield return featureManifest;
                    }
                }
            }
        }
    }
}
