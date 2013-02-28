using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.Models.Repository;
using Disco.BI.Extensions;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Models.UI.User;

namespace Disco.Web.Controllers
{
    public partial class UserController : dbAdminController
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
        public virtual ActionResult Show(string id)
        {
            var m = new Models.User.ShowModel();

            dbContext.Configuration.LazyLoadingEnabled = true;

            // Update User Cache
            // Do this first so the Database is updated if necessary
            try
            {
                Disco.BI.UserBI.UserCache.GetUser(id, dbContext, true);
            }
            catch (ArgumentException)
            {
                // Ignore if User not in Active Directory anymore
            }


            m.User = dbContext.Users.Where(um => um.Id == id).FirstOrDefault();

            m.Jobs = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true, ShowDevice = true, ShowUser = false, IsSmallTable = true, HideClosedJobs = true };
            m.Jobs.Fill(dbContext, BI.JobBI.Searching.BuildJobTableModel(dbContext).Where(j => j.UserId == id));

            m.DocumentTemplates = m.User.AvailableDocumentTemplates(dbContext, DiscoApplication.CurrentUser, DateTime.Now);

            // UI Extensions
            UIExtensions.ExecuteExtensions<UserShowModel>(this.ControllerContext, m);

            return View(m);
        }
        #endregion

    }
}
