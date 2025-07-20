using Disco.Models.Repository;
using Disco.Models.UI.Config.DeviceProfile;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Devices.ManagedGroups;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.CertificateAuthorityProvider;
using Disco.Services.Plugins.Features.CertificateProvider;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Plugins.Features.WirelessProfileProvider;
using Disco.Services.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class DeviceProfileController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Config.DeviceProfile.Show)]
        public virtual ActionResult Index(int? id)
        {
            if (id.HasValue)
            {
                var m = Database.DeviceProfiles.Where(dp => dp.Id == id.Value).Select(dp => new Models.DeviceProfile.ShowModel()
                {
                    DeviceProfile = dp,
                    DeviceCount = dp.Devices.Count(),
                    DeviceDecommissionedCount = dp.Devices.Where(d => d.DecommissionedDate.HasValue).Count()
                }).FirstOrDefault();

                if (m == null || m.DeviceProfile == null)
                    throw new ArgumentException("Invalid Device Profile Id", "id");

                m.OrganisationAddresses = Database.DiscoConfiguration.OrganisationAddresses.Addresses.OrderBy(a => a.Name).ToList();

                if (m.DeviceProfile.DefaultOrganisationAddress.HasValue)
                    m.DefaultOrganisationAddress = Database.DiscoConfiguration.OrganisationAddresses.GetAddress(m.DeviceProfile.DefaultOrganisationAddress.Value);

                DeviceProfileAssignedUsersManagedGroup assignedUsersManagedGroup;
                if (DeviceProfileAssignedUsersManagedGroup.TryGetManagedGroup(m.DeviceProfile, out assignedUsersManagedGroup))
                    m.AssignedUsersLinkedGroup = assignedUsersManagedGroup;
                DeviceProfileDevicesManagedGroup devicesManagedGroup;
                if (DeviceProfileDevicesManagedGroup.TryGetManagedGroup(m.DeviceProfile, out devicesManagedGroup))
                    m.DevicesLinkedGroup = devicesManagedGroup;

                // Ensure Specified OU Exists
                if (string.IsNullOrEmpty(m.DeviceProfile.OrganisationalUnit))
                {
                    m.OrganisationalUnitExists = true; // default container
                }
                else
                {
                    try
                    {
                        var ou = m.DeviceProfile.OrganisationalUnit;
                        var domain = ActiveDirectory.Context.GetDomainFromDistinguishedName(ou);
                        var domainController = domain.GetAvailableDomainController();
                        using (var deOU = domainController.RetrieveDirectoryEntry(ou, new string[] { "distinguishedName" }))
                        {
                            m.OrganisationalUnitExists = true;
                        }
                    }
                    catch (Exception)
                    {
                        m.OrganisationalUnitExists = false;
                    }
                }

                m.CertificateProviders = Plugins.GetPluginFeatures(typeof(CertificateProviderFeature));
                m.CertificateAuthorityProviders = Plugins.GetPluginFeatures(typeof(CertificateAuthorityProviderFeature));
                m.WirelessProfileProviders = Plugins.GetPluginFeatures(typeof(WirelessProfileProviderFeature));

                var DistributionValues = Enum.GetValues(typeof(DeviceProfile.DistributionTypes));
                m.DeviceProfileDistributionTypes = new List<SelectListItem>();
                foreach (int value in DistributionValues)
                {
                    m.DeviceProfileDistributionTypes.Add(new SelectListItem()
                    {
                        Value = value.ToString(),
                        Text = Enum.GetName(typeof(DeviceProfile.DistributionTypes), value),
                        Selected = ((int)m.DeviceProfile.DistributionType == value)
                    });
                }
                m.CanDelete = m.DeviceProfile.CanDelete(Database);
                m.CanDecommission = m.DeviceProfile.CanDecommission(Database);

                if (m.DeviceCount - m.DeviceDecommissionedCount > 0)
                    m.BulkGenerateDocumentTemplates = Database.DocumentTemplates.Where(t => !t.IsHidden).ToList();

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDeviceProfileShowModel>(ControllerContext, m);

                return View(MVC.Config.DeviceProfile.Views.Show, m);
            }
            else
            {
                var m = Models.DeviceProfile.IndexModel.Build(Database);

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDeviceProfileIndexModel>(ControllerContext, m);

                return View(m);
            }
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceProfile.Create, Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult Create()
        {
            var m = new Models.DeviceProfile.CreateModel()
            {
                DeviceProfile = new DeviceProfile()
                {
                    ComputerNameTemplate = DeviceProfile.DefaultComputerNameTemplate,
                    ProvisionADAccount = true,
                    DistributionType = DeviceProfile.DistributionTypes.OneToMany,
                    OrganisationalUnit = ActiveDirectory.Context.PrimaryDomain.DefaultComputerContainer
                }
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceProfileCreateModel>(ControllerContext, m);

            return View(m);
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceProfile.Create, Claims.Config.DeviceProfile.Configure), HttpPost]
        public virtual ActionResult Create(Models.DeviceProfile.CreateModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for Existing
                var existing = Database.DeviceProfiles.Where(m => m.Name == model.DeviceProfile.Name).FirstOrDefault();
                if (existing == null)
                {
                    model.DeviceProfile.ProvisionADAccount = true;

                    Database.DeviceProfiles.Add(model.DeviceProfile);
                    Database.SaveChanges();
                    return RedirectToAction(MVC.Config.DeviceProfile.Index(model.DeviceProfile.Id));
                }
                else
                {
                    ModelState.AddModelError("Name", "A Device Profile with this name already exists.");
                }
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceProfileCreateModel>(ControllerContext, model);

            return View(model);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.ConfigureDefaults)]
        public virtual ActionResult Defaults()
        {
            var m = new Models.DeviceProfile.DefaultsModel()
            {
                DeviceProfiles = Database.DeviceProfiles.ToList(),
                Default = Database.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId,
                DefaultAddDeviceOffline = Database.DiscoConfiguration.DeviceProfiles.DefaultAddDeviceOfflineDeviceProfileId
            };
            m.DeviceProfilesAndNone = m.DeviceProfiles.ToList();
            m.DeviceProfilesAndNone.Insert(0, new DeviceProfile() { Id = 0, Name = "<No Default>" });

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceProfileDefaultsModel>(ControllerContext, m);

            return View(m);
        }

    }
}
