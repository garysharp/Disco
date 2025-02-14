using Disco.Models.Areas.Config.UI.UserFlag;
using Disco.Models.Repository;
using Disco.Models.Services.Users.UserFlags;
using Disco.Models.UI.Config.UserFlag;
using Disco.Services.Authorization;
using Disco.Services.Exporting;
using Disco.Services.Extensions;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Users.UserFlags;
using Disco.Services.Web;
using Disco.Web.Areas.Config.Models.UserFlag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class UserFlagController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Config.UserFlag.Show)]
        public virtual ActionResult Index(int? id)
        {
            if (id.HasValue)
            {
                // Show
                var m = Database.UserFlags.Where(f => f.Id == id.Value).Select(f =>
                    new Models.UserFlag.ShowModel()
                    {
                        UserFlag = f,
                        CurrentAssignmentCount = f.UserFlagAssignments.Count(a => !a.RemovedDate.HasValue),
                        TotalAssignmentCount = f.UserFlagAssignments.Count()
                    }).FirstOrDefault();

                if (m == null)
                    throw new ArgumentException("Invalid User Flag Id");

                UserFlagUsersManagedGroup assignedUsersManagedGroup;
                if (UserFlagUsersManagedGroup.TryGetManagedGroup(m.UserFlag, out assignedUsersManagedGroup))
                    m.UsersLinkedGroup = assignedUsersManagedGroup;
                UserFlagUserDevicesManagedGroup assignedUserDevicesManagedGroup;
                if (UserFlagUserDevicesManagedGroup.TryGetManagedGroup(m.UserFlag, out assignedUserDevicesManagedGroup))
                    m.UserDevicesLinkedGroup = assignedUserDevicesManagedGroup;

                if (Authorization.Has(Claims.Config.UserFlag.Configure))
                {
                    m.Icons = UIHelpers.Icons;
                    m.ThemeColours = UIHelpers.ThemeColours;
                }

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigUserFlagShowModel>(this.ControllerContext, m);

                return View(MVC.Config.UserFlag.Views.Show, m);
            }
            else
            {
                // List Index
                var m = new Models.UserFlag.IndexModel()
                {
                    UserFlags = Database.UserFlags
                        .Select(uf => new
                        {
                            userFlag = uf,
                            assignmentCount = uf.UserFlagAssignments.Count(fa => !fa.RemovedDate.HasValue)
                        })
                        .ToDictionary(
                            pair => pair.userFlag,
                            pair => pair.assignmentCount)
                };

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigUserFlagIndexModel>(this.ControllerContext, m);

                return View(m);
            }
        }

        [DiscoAuthorizeAll(Claims.Config.UserFlag.Create, Claims.Config.UserFlag.Configure)]
        public virtual ActionResult Create()
        {
            // Default Queue
            var m = new Models.UserFlag.CreateModel()
            {
                UserFlag = new UserFlag()
                {
                    Icon = UserFlagService.RandomUnusedIcon(),
                    IconColour = UserFlagService.RandomUnusedThemeColour()
                }
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigUserFlagCreateModel>(this.ControllerContext, m);

            return View(m);
        }

        [DiscoAuthorizeAll(Claims.Config.UserFlag.Create, Claims.Config.UserFlag.Configure), HttpPost]
        public virtual ActionResult Create(Models.UserFlag.CreateModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for Existing
                var existing = Database.UserFlags.Where(m => m.Name == model.UserFlag.Name).FirstOrDefault();
                if (existing == null)
                {
                    var flag = UserFlagService.CreateUserFlag(Database, model.UserFlag);

                    return RedirectToAction(MVC.Config.UserFlag.Index(flag.Id));
                }
                else
                {
                    ModelState.AddModelError("Name", "A User Flag with this name already exists.");
                }
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigUserFlagCreateModel>(this.ControllerContext, model);

            return View(model);
        }

        #region Export

        [HttpGet]
        [DiscoAuthorizeAny(Claims.Config.UserFlag.Export)]
        public virtual ActionResult Export(Guid? exportId, int? userFlagId, bool? currentOnly)
        {
            var m = new ExportModel()
            {
                Options = UserFlagExportOptions.DefaultOptions(),
                UserFlags = UserFlagService.GetUserFlags(),
            };

            if (ExportTask.TryFromCache(exportId, out var context))
            {
                m.ExportId = exportId;
                m.ExportResult = context.Result;
            }

            if (userFlagId.HasValue && currentOnly.HasValue)
            {
                m.Options.UserFlagIds = new List<int>() { userFlagId.Value };
                m.Options.CurrentOnly = currentOnly.Value;
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigUserFlagExportModel>(ControllerContext, m);

            return View(m);
        }

        #endregion
    }
}