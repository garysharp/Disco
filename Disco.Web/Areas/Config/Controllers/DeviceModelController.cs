using Disco.Models.Repository;
using Disco.Models.UI.Config.DeviceModel;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.RepairProvider;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Plugins.Features.WarrantyProvider;
using Disco.Services.Web;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class DeviceModelController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Config.DeviceModel.Show)]
        public virtual ActionResult Index(int? id)
        {
            if (id.HasValue)
            {
                var m = Database.DeviceModels.Include("DeviceComponents").Where(dm => dm.Id == id.Value).Select(dm => new Models.DeviceModel.ShowModel()
                {
                    DeviceModel = dm,
                    DeviceCount = dm.Devices.Count(),
                    DeviceDecommissionedCount = dm.Devices.Where(d => d.DecommissionedDate.HasValue).Count()
                }).FirstOrDefault();

                if (m == null || m.DeviceModel == null)
                    throw new ArgumentException("Invalid Device Model Id", "id");

                m.WarrantyProviders = Plugins.GetPluginFeatures(typeof(WarrantyProviderFeature));
                m.RepairProviders = Plugins.GetPluginFeatures(typeof(RepairProviderFeature));

                m.DeviceComponentsModel = new Models.DeviceModel.ComponentsModel()
                {
                    DeviceModelId = m.DeviceModel.Id,
                    DeviceComponents = Database.DeviceComponents.Include("JobSubTypes").Where(dc => dc.DeviceModelId == m.DeviceModel.Id).ToList(),
                    JobSubTypes = Database.JobSubTypes.Where(jst => jst.JobTypeId == JobType.JobTypeIds.HNWar).ToList()
                };

                m.CanDelete = m.DeviceModel.CanDelete(Database);
                m.CanDecommission = m.DeviceModel.CanDecommission(Database);

                if (m.DeviceCount - m.DeviceDecommissionedCount > 0)
                    m.BulkGenerateDocumentTemplates = Database.DocumentTemplates.Where(t => !t.IsHidden).ToList();

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDeviceModelShowModel>(ControllerContext, m);

                return View(MVC.Config.DeviceModel.Views.Show, m);
            }
            else
            {
                var m = Models.DeviceModel.IndexModel.Build(Database);

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDeviceModelIndexModel>(ControllerContext, m);

                return View(m);
            }
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceModel.CreateCustom, Claims.Config.DeviceModel.Configure)]
        [HttpGet]
        public virtual ActionResult Create()
        {
            var m = new Models.DeviceModel.CreateModel();

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceModelCreateModel>(ControllerContext, m);

            return View(m);
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceModel.CreateCustom, Claims.Config.DeviceModel.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Create(Models.DeviceModel.CreateModel model)
        {
            if (ModelState.IsValid)
            {
                var deviceModel = new DeviceModel()
                {
                    Description = model.Description.NullOrTrimmed(),
                    Manufacturer = model.Manufacturer.NullOrTrimmed(),
                    Model = model.ManufacturerModel.NullOrTrimmed(),
                    ModelType = DeviceModel.CustomModelType,
                };

                Database.DeviceModels.Add(deviceModel);
                Database.SaveChanges();
                return RedirectToAction(MVC.Config.DeviceModel.Index(deviceModel.Id));
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceModelCreateModel>(ControllerContext, model);

            return View(model);
        }

        [DiscoAuthorize(Claims.Config.DeviceModel.Show)]
        public virtual ActionResult GenericComponents()
        {
            var m = new Models.DeviceModel.ComponentsModel()
            {
                DeviceComponents = Database.DeviceComponents.Include("JobSubTypes").Where(dc => !dc.DeviceModelId.HasValue).ToList(),
                JobSubTypes = Database.JobSubTypes.Where(jst => jst.JobTypeId == JobType.JobTypeIds.HNWar).ToList()
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceModelComponentsModel>(ControllerContext, m);

            return View(m);
        }
    }
}
