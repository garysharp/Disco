using Disco.Models.UI.Config.DeviceBatch;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Devices;
using Disco.Services.Devices.ManagedGroups;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Web;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class DeviceBatchController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Config.DeviceBatch.Show)]
        public virtual ActionResult Index(int? id)
        {
            Database.Configuration.LazyLoadingEnabled = true;

            if (id.HasValue)
            {
                var m = Database.DeviceBatches.Where(db => db.Id == id.Value)
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

                DeviceBatchAssignedUsersManagedGroup assignedUsersManagedGroup;
                if (DeviceBatchAssignedUsersManagedGroup.TryGetManagedGroup(m.DeviceBatch, out assignedUsersManagedGroup))
                    m.AssignedUsersLinkedGroup = assignedUsersManagedGroup;
                DeviceBatchDevicesManagedGroup devicesManagedGroup;
                if (DeviceBatchDevicesManagedGroup.TryGetManagedGroup(m.DeviceBatch, out devicesManagedGroup))
                    m.DevicesLinkedGroup = devicesManagedGroup;

                if (Authorization.Has(Claims.Config.DeviceBatch.Delete))
                    m.CanDelete = m.DeviceBatch.CanDelete(Database);

                if (Authorization.Has(Claims.Config.DeviceBatch.Configure))
                {
                    m.DeviceModels = Database.DeviceModels.ToList();
                    m.DefaultDeviceModel = m.DeviceBatch.DefaultDeviceModelId.HasValue ? m.DeviceModels.FirstOrDefault(dm => dm.Id == m.DeviceBatch.DefaultDeviceModelId.Value) : null;
                }
                else
                {
                    m.DefaultDeviceModel = m.DeviceBatch.DefaultDeviceModelId.HasValue ? Database.DeviceModels.Find(m.DeviceBatch.DefaultDeviceModelId.Value) : null;
                }

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDeviceBatchShowModel>(this.ControllerContext, m);

                return View(MVC.Config.DeviceBatch.Views.Show, m);
            }
            else
            {
                var m = Models.DeviceBatch.IndexModel.Build(Database);

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDeviceBatchIndexModel>(this.ControllerContext, m);

                return View(m);
            }
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceBatch.Create, Claims.Config.DeviceBatch.Configure)]
        public virtual ActionResult Create()
        {
            // Default Batch
            var m = new Models.DeviceBatch.CreateModel()
            {
                DeviceBatch = DeviceBatches.DefaultNewDeviceBatch(Database)
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceBatchCreateModel>(this.ControllerContext, m);

            return View(m);
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceBatch.Create, Claims.Config.DeviceBatch.Configure), HttpPost]
        public virtual ActionResult Create(Models.DeviceBatch.CreateModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for Existing
                var existing = Database.DeviceBatches.Where(m => m.Name == model.DeviceBatch.Name).FirstOrDefault();
                if (existing == null)
                {
                    Database.DeviceBatches.Add(model.DeviceBatch);
                    Database.SaveChanges();
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

        [DiscoAuthorize(Claims.Config.DeviceBatch.ShowTimeline)]
        public virtual ActionResult Timeline()
        {
            var m = new Models.DeviceBatch.TimelineModel();

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceBatchTimelineModel>(this.ControllerContext, m);

            return View();
        }

    }
}
