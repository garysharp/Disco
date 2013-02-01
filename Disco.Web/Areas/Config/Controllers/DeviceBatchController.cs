using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.BI;
using Disco.BI.Extensions;
using Disco.Web.Extensions;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class DeviceBatchController : dbAdminController
    {

        public virtual ActionResult Index(int? id)
        {
            dbContext.Configuration.LazyLoadingEnabled = true;

            if (id.HasValue)
            {
                var m = new Models.DeviceBatch.ShowModel()
                {
                    DeviceBatch = dbContext.DeviceBatches.Find(id)
                };
                if (m.DeviceBatch == null)
                {
                    return RedirectToAction(MVC.Config.DeviceBatch.Index(null));
                }
                m.CanDelete = m.DeviceBatch.CanDelete(dbContext);

                m.DeviceCount = m.DeviceBatch.Devices.Count();
                m.DeviceDecommissionedCount = m.DeviceBatch.Devices.Count(d => d.DecommissionedDate.HasValue);

                m.DeviceModels = dbContext.DeviceModels.ToSelectListItems();

                return View(MVC.Config.DeviceBatch.Views.Show, m);
            }
            else
            {
                return View(Models.DeviceBatch.IndexModel.Build(dbContext));
            }
        }

        public virtual ActionResult Create()
        {
            // Default Batch
            var m = BI.DeviceBI.BatchUtilities.DefaultNewDeviceBatch(dbContext);
            return View(m);
        }

        [HttpPost]
        public virtual ActionResult Create(Disco.Models.Repository.DeviceBatch model)
        {
            if (ModelState.IsValid)
            {
                // Check for Existing
                var existing = dbContext.DeviceBatches.Where(m => m.Name == model.Name).FirstOrDefault();
                if (existing == null)
                {
                    dbContext.DeviceBatches.Add(model);
                    dbContext.SaveChanges();
                    return RedirectToAction(MVC.Config.DeviceBatch.Index(model.Id));
                }
                else
                {
                    ModelState.AddModelError("Name", "A Device Batch with this name already exists.");
                }
            }

            return View(model);
        }

        public virtual ActionResult Timeline()
        {
            return View();
        }

    }
}
