using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.BI;
using Disco.BI.Extensions;
using Disco.Web.Extensions;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Models.UI.Config.DeviceBatch;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class DeviceBatchController : dbAdminController
    {

        public virtual ActionResult Index(int? id)
        {
            dbContext.Configuration.LazyLoadingEnabled = true;

            if (id.HasValue)
            {
                var m = dbContext.DeviceBatches.Where(db => db.Id == id.Value)
                    .Select(db => new Models.DeviceBatch.ShowModel()
                    {
                        DeviceBatch = db,
                        DeviceCount = db.Devices.Count(),
                        DeviceDecommissionedCount = db.Devices.Count(d => d.DecommissionedDate.HasValue)
                    }).FirstOrDefault();

                if (m == null || m.DeviceBatch == null)
                    throw new ArgumentException("Invalid Device Batch Id", "id");

                m.DeviceModelMembers = m.DeviceBatch.Devices.GroupBy(d => d.DeviceModel).Select(dG => new Models.DeviceBatch._ShowModelMembership()
                {
                    DeviceModel = dG.Key,
                    DeviceCount = dG.Count(),
                    DeviceDecommissionedCount = dG.Count(d => d.DecommissionedDate.HasValue)
                }).ToArray().Cast<ConfigDeviceBatchShowModelMembership>().ToList();

                m.CanDelete = m.DeviceBatch.CanDelete(dbContext);

                m.DeviceModels = dbContext.DeviceModels.ToList();

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDeviceBatchShowModel>(this.ControllerContext, m);

                return View(MVC.Config.DeviceBatch.Views.Show, m);
            }
            else
            {
                var m = Models.DeviceBatch.IndexModel.Build(dbContext);

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDeviceBatchIndexModel>(this.ControllerContext, m);

                return View(m);
            }
        }

        public virtual ActionResult Create()
        {
            // Default Batch
            var m = new Models.DeviceBatch.CreateModel()
            {
                DeviceBatch = BI.DeviceBI.BatchUtilities.DefaultNewDeviceBatch(dbContext)
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceBatchCreateModel>(this.ControllerContext, m);

            return View(m);
        }

        [HttpPost]
        public virtual ActionResult Create(Models.DeviceBatch.CreateModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for Existing
                var existing = dbContext.DeviceBatches.Where(m => m.Name == model.DeviceBatch.Name).FirstOrDefault();
                if (existing == null)
                {
                    dbContext.DeviceBatches.Add(model.DeviceBatch);
                    dbContext.SaveChanges();
                    return RedirectToAction(MVC.Config.DeviceBatch.Index(model.DeviceBatch.Id));
                }
                else
                {
                    ModelState.AddModelError("Name", "A Device Batch with this name already exists.");
                }
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceBatchCreateModel>(this.ControllerContext, model);

            return View(model);
        }

        public virtual ActionResult Timeline()
        {
            var m = new Models.DeviceBatch.TimelineModel();

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceBatchTimelineModel>(this.ControllerContext, m);

            return View();
        }

    }
}
