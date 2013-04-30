using Disco.Models.UI.Config.Organisation;
using Disco.Services.Plugins.Features.UIExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class OrganisationController : dbAdminController
    {
        //
        // GET: /Config/Organisation/

        public virtual ActionResult Index()
        {
            var viewModel = new Models.Organisation.IndexModel();

            viewModel.OrganisationName = dbContext.DiscoConfiguration.OrganisationName;
            viewModel.MultiSiteMode = dbContext.DiscoConfiguration.MultiSiteMode;
            viewModel.OrganisationAddresses = dbContext.DiscoConfiguration.OrganisationAddresses.Addresses;

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigOrganisationIndexModel>(this.ControllerContext, viewModel);

            return View(viewModel);
        }

    }
}
