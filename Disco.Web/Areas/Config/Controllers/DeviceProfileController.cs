using Disco.BI.Extensions;
using Disco.Models.Repository;
using Disco.Models.UI.Config.DeviceProfile;
using Disco.Services.Authorization;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.CertificateProvider;
using Disco.Services.Plugins.Features.UIExtension;
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

                m.OrganisationAddresses = Database.DiscoConfiguration.OrganisationAddresses.Addresses;

                if (m.DeviceProfile.DefaultOrganisationAddress.HasValue)
                    m.DefaultOrganisationAddress = Database.DiscoConfiguration.OrganisationAddresses.GetAddress(m.DeviceProfile.DefaultOrganisationAddress.Value);

                m.CertificateProviders = Plugins.GetPluginFeatures(typeof(CertificateProviderFeature));

                var DistributionValues = Enum.GetValues(typeof(Disco.Models.Repository.DeviceProfile.DistributionTypes));
                m.DeviceProfileDistributionTypes = new List<SelectListItem>();
                foreach (int value in DistributionValues)
                {
                    m.DeviceProfileDistributionTypes.Add(new SelectListItem()
                    {
                        Value = value.ToString(),
                        Text = Enum.GetName(typeof(Disco.Models.Repository.DeviceProfile.DistributionTypes), value),
                        Selected = ((int)m.DeviceProfile.DistributionType == value)
                    });
                }
                m.CanDelete = m.DeviceProfile.CanDelete(Database);

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDeviceProfileShowModel>(this.ControllerContext, m);

                return View(MVC.Config.DeviceProfile.Views.Show, m);
            }
            else
            {
                var m = Models.DeviceProfile.IndexModel.Build(Database);

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDeviceProfileIndexModel>(this.ControllerContext, m);

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
                    ComputerNameTemplate = "DeviceProfile.ShortName + '-' + SerialNumber",
                    ProvisionADAccount = true,
                    DistributionType = DeviceProfile.DistributionTypes.OneToMany
                }
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceProfileCreateModel>(this.ControllerContext, m);

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
            UIExtensions.ExecuteExtensions<ConfigDeviceProfileCreateModel>(this.ControllerContext, model);

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
            m.DeviceProfilesAndNone.Insert(0, new Disco.Models.Repository.DeviceProfile() { Id = 0, Name = "<No Default>" });

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceProfileDefaultsModel>(this.ControllerContext, m);

            return View(m);
        }

    }
}
