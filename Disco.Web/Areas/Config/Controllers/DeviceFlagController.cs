using Disco.Models.Areas.Config.UI.DeviceFlag;
using Disco.Models.Services.Devices.DeviceFlag;
using Disco.Models.UI.Config.DeviceFlag;
using Disco.Services.Authorization;
using Disco.Services.Devices.DeviceFlags;
using Disco.Services.Exporting;
using Disco.Services.Extensions;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Web;
using Disco.Web.Areas.Config.Models.DeviceFlag;
using Disco.Web.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class DeviceFlagController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Config.DeviceFlag.Show)]
        public virtual ActionResult Index(int? id)
        {
            if (id.HasValue)
            {
                // Show
                var m = Database.DeviceFlags.Where(f => f.Id == id.Value).Select(f =>
                    new ShowModel()
                    {
                        DeviceFlag = f,
                        CurrentAssignmentCount = f.DeviceFlagAssignments.Count(a => !a.RemovedDate.HasValue),
                        TotalAssignmentCount = f.DeviceFlagAssignments.Count()
                    }).FirstOrDefault();

                if (m == null)
                    throw new ArgumentException("Invalid Device Flag Id");

                if (DeviceFlagDevicesManagedGroup.TryGetManagedGroup(m.DeviceFlag, out var devicesManagedGroup))
                    m.DevicesLinkedGroup = devicesManagedGroup;
                if (DeviceFlagDeviceAssignedUsersManagedGroup.TryGetManagedGroup(m.DeviceFlag, out var assignedUsersManagedGroup))
                    m.AssignedUserLinkedGroup = assignedUsersManagedGroup;

                if (Authorization.Has(Claims.Config.DeviceFlag.Configure))
                {
                    m.Icons = UIHelpers.Icons;
                    m.ThemeColours = UIHelpers.ThemeColours;
                }

                var (_, permission) = DeviceFlagService.GetDeviceFlag(m.DeviceFlag.Id);
                m.Permission = permission;

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDeviceFlagShowModel>(ControllerContext, m);

                return View(MVC.Config.DeviceFlag.Views.Show, m);
            }
            else
            {
                // List Index
                var m = new IndexModel()
                {
                    DeviceFlags = Database.DeviceFlags
                        .Select(uf => new
                        {
                            flag = uf,
                            assignmentCount = uf.DeviceFlagAssignments.Count(fa => !fa.RemovedDate.HasValue)
                        })
                        .ToDictionary(
                            pair => pair.flag,
                            pair => pair.assignmentCount)
                };

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDeviceFlagIndexModel>(ControllerContext, m);

                return View(m);
            }
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceFlag.Create, Claims.Config.DeviceFlag.Configure)]
        public virtual ActionResult Create()
        {
            // Default Queue
            var m = new CreateModel();

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceFlagCreateModel>(ControllerContext, m);

            return View(m);
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceFlag.Create, Claims.Config.DeviceFlag.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Create(CreateModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for Existing
                var existing = Database.DeviceFlags.Where(m => m.Name == model.Name).FirstOrDefault();
                if (existing == null)
                {
                    var flag = DeviceFlagService.CreateDeviceFlag(Database, model.Name, model.Description);

                    return RedirectToAction(MVC.Config.DeviceFlag.Index(flag.Id));
                }
                else
                {
                    ModelState.AddModelError("Name", "A Device Flag with this name already exists.");
                }
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceFlagCreateModel>(ControllerContext, model);

            return View(model);
        }

        #region Export

        [DiscoAuthorizeAny(Claims.Config.DeviceFlag.Export), HttpGet]
        public virtual ActionResult Export(Guid? exportId, int? deviceFlagId, bool? currentOnly)
        {
            var m = new ExportModel()
            {
                Options = Database.DiscoConfiguration.DeviceFlags.LastExportOptions,
                DeviceFlags = DeviceFlagService.GetDeviceFlags().Select(f => f.flag).ToList(),
            };

            m.Fields = ExportFieldsModel.Create(m.Options, DeviceFlagExportOptions.DefaultOptions(), nameof(DeviceFlagExportOptions.CurrentOnly));
            m.Fields.AddCustomUserDetails(o => o.UserDetailCustom);

            if (ExportTask.TryFromCache(exportId, out var context))
            {
                m.ExportId = context.Id;
                m.ExportResult = context.Result;
            }

            if (deviceFlagId.HasValue && currentOnly.HasValue)
            {
                m.Options.DeviceFlagIds = new List<int>() { deviceFlagId.Value };
                m.Options.CurrentOnly = currentOnly.Value;
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceFlagExportModel>(ControllerContext, m);

            return View(m);
        }

        #endregion
    }
}