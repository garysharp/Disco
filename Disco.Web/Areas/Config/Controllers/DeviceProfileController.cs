using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.Data.Configuration;
using Disco.BI;
using Disco.BI.Extensions;
using Disco.Services.Plugins.Features.CertificateProvider;
using Disco.Services.Plugins;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class DeviceProfileController : dbAdminController
    {
        public virtual ActionResult Index(int? id)
        {
            if (id.HasValue)
            {
                var m = new Models.DeviceProfile.ShowModel()
                {
                    DeviceProfile = dbContext.DeviceProfiles.Find(id.Value),
                    OrganisationAddresses = dbContext.DiscoConfiguration.OrganisationAddresses.Addresses,
                    CertificateProviders = Plugins.GetPluginFeatures(typeof(CertificateProviderFeature))
                };

                //m.Devices = BI.DeviceBI.SelectDeviceSearchResultItem(dbContext.Devices.Where(d => d.DeviceProfileId == m.DeviceProfile.Id));

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
                m.CanDelete = m.DeviceProfile.CanDelete(dbContext);

                // Removed 2012-06-14 G# - Properties moved to DeviceProfile model & DB Migrated in DBv3.
                //var config = m.DeviceProfile.Configuration(dbContext);
                //m.AllocateWirelessCertificate = m.DeviceProfile.AllocateWirelessCertificate;
                //m.OrganisationalUnit = m.DeviceProfile.OrganisationalUnit;
                //m.ComputerNameTemplate = m.DeviceProfile.ComputerNameTemplate;

                return View(MVC.Config.DeviceProfile.Views.Show, m);
            }
            else
            {
                return View(Models.DeviceProfile.IndexModel.Build(dbContext));
            }
        }

        public virtual ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public virtual ActionResult Create(Disco.Models.Repository.DeviceProfile model)
        {
            if (ModelState.IsValid)
            {
                // Check for Existing
                var existing = dbContext.DeviceProfiles.Where(m => m.Name == model.Name).FirstOrDefault();
                if (existing == null)
                {
                    model.ProvisionADAccount = true;

                    dbContext.DeviceProfiles.Add(model);
                    dbContext.SaveChanges();
                    return RedirectToAction(MVC.Config.DeviceProfile.Index(model.Id));
                }
                else
                {
                    ModelState.AddModelError("Name", "A Device Profile with this name already exists.");
                }
            }

            return View(model);
        }

        public virtual ActionResult Defaults()
        {
            var m = new Models.DeviceProfile.DefaultsModel()
            {
                DeviceProfiles = dbContext.DeviceProfiles.ToList(),
                Default = dbContext.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId,
                DefaultAddDeviceOffline = dbContext.DiscoConfiguration.DeviceProfiles.DefaultAddDeviceOfflineDeviceProfileId
            };
            m.DeviceProfilesAndNone = m.DeviceProfiles.ToList();
            m.DeviceProfilesAndNone.Insert(0, new Disco.Models.Repository.DeviceProfile() { Id = 0, Name = "<No Default>" });

            return View(m);
        }

    }
}
