using Disco.BI.Extensions;
using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.UI.User;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Authorization.Roles;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Users;
using Disco.Services.Web;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Controllers
{
    public partial class UserController : AuthorizedDatabaseController
    {
        #region Index
        public virtual ActionResult Index()
        {
            var m = new Models.User.IndexModel();

            // UI Extensions
            UIExtensions.ExecuteExtensions<UserIndexModel>(this.ControllerContext, m);

            return View();
        }
        #endregion

        #region Show
        [DiscoAuthorize(Claims.User.Show)]
        public virtual ActionResult Show(string id)
        {
            var m = new Models.User.ShowModel();

            Database.Configuration.LazyLoadingEnabled = true;

            // Update User Cache
            // Do this first so the Database is updated if necessary
            try
            {
                UserService.GetUser(id, Database, true);
            }
            catch (ArgumentException)
            {
                // Ignore if User not in Active Directory anymore
            }

            m.User = Database.Users
                .Include("DeviceUserAssignments.Device.DeviceModel").Include("UserAttachments")
                .FirstOrDefault(um => um.Id == id);

            if (m.User == null)
                throw new ArgumentException("Unknown User Id", "id");

            if (Authorization.Has(Claims.User.ShowJobs))
            {
                m.Jobs = new JobTableModel()
                {
                    ShowStatus = true,
                    ShowDevice = true,
                    ShowUser = false,
                    IsSmallTable = false,
                    HideClosedJobs = true,
                    EnablePaging = false
                };
                m.Jobs.Fill(Database, Disco.Services.Searching.Search.BuildJobTableModel(Database).Where(j => j.UserId == id).OrderByDescending(j => j.Id), true);
            }

            try
            {
                if (Authorization.Has(Claims.User.ShowAuthorization))
                {
                    m.AuthorizationToken = UserService.GetAuthorization(id);
                    var claims = m.AuthorizationToken.RoleTokens.Cast<RoleToken>().Select(rt => rt.Claims).ToArray();
                    if (claims.Length > 0)
                        m.ClaimNavigator = Claims.RoleClaimNavigator.BuildClaimTree(claims);
                }
            }
            catch (ArgumentException)
            {
                // Ignore if User not in Active Directory anymore
            }

            if (Authorization.Has(Claims.User.Actions.GenerateDocuments))
                m.DocumentTemplates = m.User.AvailableDocumentTemplates(Database, UserService.CurrentUser, DateTime.Now);

            // UI Extensions
            UIExtensions.ExecuteExtensions<UserShowModel>(this.ControllerContext, m);

            return View(m);
        }
        #endregion

    }
}
