using Disco.Services;
using Disco.Services.Interop.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Controllers
{
    public partial class UserController
    {
        internal static void T4MVCAddUserIdRouteValues(T4MVC_System_Web_Mvc_ActionResult callInfo, string UserId)
        {
            var slashIndex = UserId.IndexOf('\\');

            if (slashIndex < 0)
                throw new ArgumentException("The User Id is not in the correct format ({Domain}\\{Id})", "id");

            string userDomain = UserId.Substring(0, slashIndex);
            if (userDomain.Equals(ActiveDirectory.Context.PrimaryDomain.NetBiosName, StringComparison.OrdinalIgnoreCase))
                userDomain = null; // Url doesn't contain Domain if it is the default.

            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", UserId.Substring(slashIndex + 1));
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "Domain", userDomain);
        }

        [NonAction]
        public virtual System.Web.Mvc.ActionResult Show(string id)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Show);

            T4MVCAddUserIdRouteValues(callInfo, id);

            return callInfo;
        }
    }
}

namespace Disco.Web.Areas.API.Controllers
{
    public partial class UserController
    {
        [NonAction]
        public virtual ActionResult AttachmentUpload(string id, string Comments)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.AttachmentUpload);

            Disco.Web.Controllers.UserController.T4MVCAddUserIdRouteValues(callInfo, id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "Comments", Comments);

            return callInfo;
        }


        [NonAction]
        public virtual ActionResult Attachments(string id)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Attachments);

            Disco.Web.Controllers.UserController.T4MVCAddUserIdRouteValues(callInfo, id);

            return callInfo;
        }

        [NonAction]
        public virtual ActionResult GeneratePdf(string id, string DocumentTemplateId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GeneratePdf);

            Disco.Web.Controllers.UserController.T4MVCAddUserIdRouteValues(callInfo, id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "DocumentTemplateId", DocumentTemplateId);

            return callInfo;
        }
    }
}