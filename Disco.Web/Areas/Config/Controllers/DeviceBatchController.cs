using Disco.Models.Repository;
using Disco.Models.UI.Config.DeviceBatch;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Devices;
using Disco.Services.Devices.ManagedGroups;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Web;
using Disco.Web.Areas.Config.Models.DeviceBatch;
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
                var m = Database.DeviceBatches
                    .Include(nameof(DeviceBatch.DeviceBatchAttachments))
                    .Where(db => db.Id == id.Value)
                    .Select(db => new ShowModel()
                    {
                        DeviceBatch = db,
                        DeviceCount = db.Devices.Count(),
                        DeviceDecommissionedCount = db.Devices.Count(d => d.DecommissionedDate.HasValue)
                    }).FirstOrDefault();

                if (m == null || m.DeviceBatch == null)
                    throw new ArgumentException("Invalid Device Batch Id", "id");

                m.DeviceModelMembers = m.DeviceBatch.Devices.GroupBy(d => d.DeviceModel).Select(dG => new _ShowModelMembership()
                {
                    DeviceModel = dG.Key,
                    DeviceCount = dG.Count(),
                    DeviceDecommissionedCount = dG.Count(d => d.DecommissionedDate.HasValue)
                }).ToArray().Cast<ConfigDeviceBatchShowModelMembership>().ToList();

                if (DeviceBatchAssignedUsersManagedGroup.TryGetManagedGroup(m.DeviceBatch, out var assignedUsersManagedGroup))
                    m.AssignedUsersLinkedGroup = assignedUsersManagedGroup;
                if (DeviceBatchDevicesManagedGroup.TryGetManagedGroup(m.DeviceBatch, out var devicesManagedGroup))
                    m.DevicesLinkedGroup = devicesManagedGroup;

                m.CanDelete = m.DeviceBatch.CanDelete(Database);
                m.CanDecommission = m.DeviceBatch.CanDecommission(Database);

                if (Authorization.Has(Claims.Config.DeviceBatch.Configure))
                {
                    m.DeviceModels = Database.DeviceModels.ToList();
                    m.DefaultDeviceModel = m.DeviceBatch.DefaultDeviceModelId.HasValue ? m.DeviceModels.FirstOrDefault(dm => dm.Id == m.DeviceBatch.DefaultDeviceModelId.Value) : null;
                }
                else
                {
                    m.DefaultDeviceModel = m.DeviceBatch.DefaultDeviceModelId.HasValue ? Database.DeviceModels.Find(m.DeviceBatch.DefaultDeviceModelId.Value) : null;
                }

                if (m.DeviceModelMembers.Any(g => g.DeviceCount - g.DeviceDecommissionedCount > 0))
                    m.BulkGenerateDocumentTemplates = Database.DocumentTemplates.Where(t => !t.IsHidden).ToList();

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDeviceBatchShowModel>(ControllerContext, m);

                return View(MVC.Config.DeviceBatch.Views.Show, m);
            }
            else
            {
                var m = Models.DeviceBatch.IndexModel.Build(Database);

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDeviceBatchIndexModel>(ControllerContext, m);

                return View(m);
            }
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceBatch.Create, Claims.Config.DeviceBatch.Configure)]
        public virtual ActionResult Create()
        {
            // Default Batch
            var m = new CreateModel()
            {
                PurchaseDate = DateTime.Today,
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceBatchCreateModel>(ControllerContext, m);

            return View(m);
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceBatch.Create, Claims.Config.DeviceBatch.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Create(CreateModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for Existing
                var alreadyExists = Database.DeviceBatches.Any(m => m.Name == model.Name);
                if (!alreadyExists)
                {
                    var batch = new DeviceBatch()
                    {
                        Name = model.Name,
                        PurchaseDate = model.PurchaseDate,
                    };
                    Database.DeviceBatches.Add(batch);
                    Database.SaveChanges();
                    return RedirectToAction(MVC.Config.DeviceBatch.Index(batch.Id));
                }
                else
                {
                    ModelState.AddModelError(nameof(CreateModel.Name), "A Device Batch with this name already exists.");
                }
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceBatchCreateModel>(ControllerContext, model);

            return View(model);
        }

        [DiscoAuthorize(Claims.Config.DeviceBatch.ShowTimeline)]
        public virtual ActionResult Timeline()
        {
            var m = new TimelineModel();

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceBatchTimelineModel>(ControllerContext, m);

            return View();
        }

    }
}
