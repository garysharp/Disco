using Disco.Models.Services.Authorization;
using Disco.Models.UI.Config.AuthorizationRole;
using Disco.Services.Authorization;
using Disco.Services.Authorization.Roles;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Users;
using Disco.Services.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    [DiscoAuthorize(Claims.DiscoAdminAccount)]
    public partial class AuthorizationRoleController : AuthorizedDatabaseController
    {
        public virtual ActionResult Index(int? id)
        {
            if (id.HasValue)
            {
                // Show
                var ar = Database.AuthorizationRoles.Find(id.Value);

                if (ar == null)
                    throw new ArgumentException("Invalid Authorization Role Id");

                var token = RoleToken.FromAuthorizationRole(ar);
                var subjects = token.SubjectIds == null ? new List<Models.AuthorizationRole.ShowModel.SubjectDescriptor>() :
                    token.SubjectIds.Select(subjectId => ActiveDirectory.RetrieveObject(subjectId))
                    .Where(item => item != null)
                    .Select(item => Models.AuthorizationRole.ShowModel.SubjectDescriptor.FromActiveDirectoryObject(item))
                    .OrderBy(item => item.Name).ToList();

                var m = new Models.AuthorizationRole.ShowModel()
                {
                    Token = token,
                    Subjects = subjects,
                    ClaimNavigator = Claims.RoleClaimNavigator.BuildClaimTree(token.Claims)
                };


                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigAuthorizationRoleShowModel>(this.ControllerContext, m);

                return View(MVC.Config.AuthorizationRole.Views.Show, m);
            }
            else
            {
                // List Index
                var ars = Database.AuthorizationRoles.ToList()
                    .Select(ar => RoleToken.FromAuthorizationRole(ar)).Cast<IRoleToken>().ToList();

                var m = new Models.AuthorizationRole.IndexModel()
                {
                    Tokens = ars
                };

                // UI Extensions
                UIExtensions.ExecuteExtensions<ConfigAuthorizationRoleIndexModel>(this.ControllerContext, m);

                return View(m);
            }
        }

        public virtual ActionResult Create()
        {
            // Default Role
            var m = new Models.AuthorizationRole.CreateModel()
            {
                AuthorizationRole = new Disco.Models.Repository.AuthorizationRole()
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigAuthorizationRoleCreateModel>(this.ControllerContext, m);

            return View(m);
        }

        [HttpPost]
        public virtual ActionResult Create(Models.AuthorizationRole.CreateModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for Existing
                var existing = Database.AuthorizationRoles.Where(m => m.Name == model.AuthorizationRole.Name).FirstOrDefault();
                if (existing == null)
                {
                    var roleId = UserService.CreateAuthorizationRole(Database, model.AuthorizationRole);

                    return RedirectToAction(MVC.Config.AuthorizationRole.Index(roleId));
                }
                else
                {
                    ModelState.AddModelError("Name", "An Authorization Role with this name already exists.");
                }
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigAuthorizationRoleCreateModel>(this.ControllerContext, model);

            return View(model);
        }
    }
}
