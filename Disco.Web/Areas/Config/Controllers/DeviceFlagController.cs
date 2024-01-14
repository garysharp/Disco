using Disco.Models.Areas.Config.UI.DeviceFlag;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.DeviceFlag;
using Disco.Models.UI.Config.DeviceFlag;
using Disco.Services.Authorization;
using Disco.Services.Devices.DeviceFlags;
using Disco.Services.Exporting;
using Disco.Services.Extensions;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Web;
using Disco.Web.Areas.Config.Models.DeviceFlag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigDeviceFlagShowModel>(this.ControllerContext, m);

                return View(MVC.Config.DeviceFlag.Views.Show, m);
            }
            else
            {
                // List Index
                var m = new Models.DeviceFlag.IndexModel()
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
                UIExtensions.ExecuteExtensions<ConfigDeviceFlagIndexModel>(this.ControllerContext, m);

                return View(m);
            }
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceFlag.Create, Claims.Config.DeviceFlag.Configure)]
        public virtual ActionResult Create()
        {
            // Default Queue
            var m = new CreateModel()
            {
                DeviceFlag = new DeviceFlag()
                {
                    Icon = DeviceFlagService.RandomUnusedIcon(),
                    IconColour = DeviceFlagService.RandomUnusedThemeColour()
                }
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceFlagCreateModel>(this.ControllerContext, m);

            return View(m);
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceFlag.Create, Claims.Config.DeviceFlag.Configure), HttpPost]
        public virtual ActionResult Create(CreateModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for Existing
                var existing = Database.DeviceFlags.Where(m => m.Name == model.DeviceFlag.Name).FirstOrDefault();
                if (existing == null)
                {
                    var flag = DeviceFlagService.CreateDeviceFlag(Database, model.DeviceFlag);

                    return RedirectToAction(MVC.Config.DeviceFlag.Index(flag.Id));
                }
                else
                {
                    ModelState.AddModelError("Name", "A Device Flag with this name already exists.");
                }
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceFlagCreateModel>(this.ControllerContext, model);

            return View(model);
        }

        #region Export

        [DiscoAuthorizeAny(Claims.Config.DeviceFlag.Export), HttpGet]
        public virtual ActionResult Export(string DownloadId, int? DeviceFlagId, bool? CurrentOnly)
        {
            var m = new ExportModel()
            {
                Options = DeviceFlagExportOptions.DefaultOptions(),
                DeviceFlags = DeviceFlagService.GetDeviceFlags(),
            };

            if (!string.IsNullOrWhiteSpace(DownloadId))
            {
                string key = string.Format(API.Controllers.DeviceFlagController.ExportSessionCacheKey, DownloadId);
                var context = HttpRuntime.Cache.Get(key) as ExportTaskContext<DeviceFlagExportOptions>;

                if (context != null)
                {
                    m.ExportSessionResult = context.Result;
                    m.ExportSessionId = DownloadId;
                }
            }

            if (DeviceFlagId.HasValue && CurrentOnly.HasValue)
            {
                m.Options.DeviceFlagIds = new List<int>() { DeviceFlagId.Value };
                m.Options.CurrentOnly = CurrentOnly.Value;
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigDeviceFlagExportModel>(this.ControllerContext, m);

            return View(m);
        }

        #endregion
    }
}