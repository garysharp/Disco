using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.Services.Plugins.Features.WarrantyProvider;
using Disco.Services.Plugins;
using Disco.BI.Extensions;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class DeviceModelController : dbAdminController
    {
        public virtual ActionResult Index(int? id)
        {
            if (id.HasValue)
            {
                var m = new Models.DeviceModel.ShowModel()
                {
                    DeviceModel = dbContext.DeviceModels.Include("DeviceComponents.JobSubTypes").Where(dm => dm.Id == id.Value).FirstOrDefault(),
                    WarrantyProviders = Plugins.GetPluginFeatures(typeof(WarrantyProviderFeature))
                };

                m.DeviceComponentsModel = new Models.DeviceModel.DeviceComponentsModel()
                {
                    DeviceModelId = m.DeviceModel.Id,
                    DeviceComponents = m.DeviceModel.DeviceComponents.ToList(),
                    JobSubTypes = dbContext.JobSubTypes.Where(jst => jst.JobTypeId == Disco.Models.Repository.JobType.JobTypeIds.HNWar).ToList()
                };

                m.CanDelete = m.DeviceModel.CanDelete(dbContext);

                //m.Devices = BI.DeviceBI.SelectDeviceSearchResultItem(dbContext.Devices.Where(d => d.DeviceModelId == m.DeviceModel.Id));

                //m.Devices = dbContext.Devices.Include("DeviceModel").Include("DeviceProfile").Include("AssignedUser")
                //    .Where(d => d.DeviceModelId == m.DeviceModel.Id).ToList();

                return View(MVC.Config.DeviceModel.Views.Show, m);
            }
            else
            {
                return View(Models.DeviceModel.IndexModel.Build(dbContext));
            }
        }

        public virtual ActionResult GenericComponents()
        {
            var m = new Models.DeviceModel.DeviceComponentsModel()
            {
                DeviceComponents = dbContext.DeviceComponents.Include("JobSubTypes").Where(dc => !dc.DeviceModelId.HasValue).ToList(),
                JobSubTypes = dbContext.JobSubTypes.Where(jst => jst.JobTypeId == Disco.Models.Repository.JobType.JobTypeIds.HNWar).ToList()
            };

            return View(m);
        }
    }
}
